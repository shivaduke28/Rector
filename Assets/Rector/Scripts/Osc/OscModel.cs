using System;
using System.Collections.Generic;
using OscJack;
using R3;

namespace Rector.Osc
{
    /// <summary>
    /// OSC の受信口。MidiModel と同じく「1つのモデルが全部受けてノードに配る」形。
    ///
    /// OscJack のディスパッチャはアドレス完全一致の辞書引きだが、空文字で登録すると
    /// 全メッセージを受け取るモニタになる。アドレスごとの購読はノード側で Where するので、
    /// ここではそのモニタ口を1本だけ張る。
    /// </summary>
    /// <remarks>
    /// OscMaster.GetSharedServer は作ったサーバーを閉じる手段を持たない。ポートを設定で
    /// 変えられるようにするため、OscServer は自前で new して自前で Dispose する。
    /// </remarks>
    public sealed class OscModel : IInitializable, IDisposable
    {
        // 受信スレッドからメインスレッドへ渡す取りこぼしの上限。
        // 溢れるのはフレームが詰まっているか送信側が暴れているかで、
        // どちらにせよ古い値を持ち越しても意味が無いので前から捨てる
        const int MaxQueued = 1024;

        // 空文字はディスパッチャのモニタ口。アドレス指定の購読とは別枠で全メッセージが来る
        const string MonitorAddress = "";

        readonly Subject<OscMessage> messages = new();

        /// <summary>メインスレッドからのみ流れる。</summary>
        public Observable<OscMessage> Messages => messages;

        // 実際にソケットが開いているか。設定を On にしても bind に失敗すれば false のまま。
        // 設定ページが「送り先」と「使えません」を出し分けるために見る
        readonly ReactiveProperty<bool> isListening = new(false);
        public ReadOnlyReactiveProperty<bool> IsListening => isListening;

        readonly OscInputSetting setting;
        readonly CompositeDisposable disposable = new();

        readonly object queueLock = new();
        readonly Queue<OscMessage> queue = new();
        readonly List<OscMessage> drained = new();
        bool overflowed;

        OscServer server;
        bool loggedOverflow;

        public OscModel(OscInputSetting setting)
        {
            this.setting = setting;
        }

        public void Initialize()
        {
            setting.Config.Subscribe(Apply).AddTo(disposable);
            Observable.EveryUpdate(UnityFrameProvider.Update).Subscribe(_ => Drain()).AddTo(disposable);
        }

        void Apply(OscInputConfig config)
        {
            Close();

            // Reload 前の初期値でも呼ばれる。ここで既定ポートを開いてしまうと、
            // 保存値が別の番号だったときに使わない口を一度 bind することになる
            if (!config.Loaded) return;

            // 溢れの通知は一度きりだが、設定を変えたら状況が変わる。
            // 直したあとに再発したとき無言にならないよう、ここで戻す
            loggedOverflow = false;

            if (!config.Enabled)
            {
                // 受信を切ると OscServer のスレッドごと畳まれる。使わない人が
                // 100ms ごとのタイムアウトを回し続けずに済む
                RectorLogger.OscDisabled();
                return;
            }

            try
            {
                var created = new OscServer(config.Port);
                // AddCallback まで通ってから server を持つ。途中で失敗したものを抱えると、
                // 次の Close の RemoveCallback が未登録アドレスを引いて KeyNotFoundException になる
                created.MessageDispatcher.AddCallback(MonitorAddress, OnMessage);
                server = created;
                isListening.Value = true;
                RectorLogger.OscListening(config.Port, OscLocalAddress.GetIPv4Addresses());
            }
            catch (Exception e)
            {
                // OscServer は ctor の中で Bind するので、使用中のポートだとここに来る。
                // 握らないと RectorInstaller の初期化ごと落ちる
                RectorLogger.OscBindFailed(config.Port, e.Message);
            }
        }

        void Close()
        {
            isListening.Value = false;
            if (server == null) return;

            server.MessageDispatcher.RemoveCallback(MonitorAddress, OnMessage);
            server.Dispose();
            server = null;
        }

        // 受信スレッドから呼ばれる。ここで例外を出すと OscServer.ServerLoop の catch に飲まれて
        // 受信スレッドごと終了し、次の Apply まで OSC が無言で止まる。
        // R3 も Unity API も触らないこと(RectorLogger は Time.realtimeSinceStartup を読むので不可)
        void OnMessage(string address, OscDataHandle data)
        {
            // OscDataHandle はパケット間で使い回されるバッファを指している。持ち出さず、ここで読み切る
            var count = data.GetElementCount();
            var value = count > 0 ? data.GetElementAsFloat(0) : 0f;
            var message = new OscMessage(address, value, count > 0 && (value != 0f || IsNumeric(data)));

            lock (queueLock)
            {
                if (queue.Count >= MaxQueued)
                {
                    queue.Dequeue();
                    overflowed = true;
                }

                queue.Enqueue(message);
            }
        }

        /// <summary>第1引数が数値(int/float)か。0 に読めたときだけ呼ぶこと。</summary>
        /// <remarks>
        /// OscJack は型タグを外に出さず、文字列も blob も GetElementAsFloat が 0 を返す。
        /// 「本当に数値の 0」と「数値ではないもの」を分けられるのは文字列化した結果だけ。
        /// 確保を伴うので、0 以外(＝数値だと確定している)では呼ばない。
        ///
        /// GetElementAsString の文字列走査は OscDataHandle.Scan が既に同じ範囲を歩いたあとなので、
        /// ここで新たにバッファ外へ出ることはない。
        /// </remarks>
        static bool IsNumeric(OscDataHandle data)
            => float.TryParse(data.GetElementAsString(0), out _);

        void Drain()
        {
            bool didOverflow;
            lock (queueLock)
            {
                while (queue.Count > 0)
                {
                    drained.Add(queue.Dequeue());
                }

                didOverflow = overflowed;
                overflowed = false;
            }

            // 必ずロックの外で流す。OnNext はノード側のコードを同期で走らせるので、
            // ロックを持ったままだと queueLock -> _callbackMap の順で待つ経路ができてしまう。
            // 受信スレッドは Dispatch が _callbackMap を持ったまま OnMessage を呼ぶため
            // _callbackMap -> queueLock の順で、逆順の待ちが揃うと ABBA デッドロックになる
            foreach (var message in drained)
            {
                messages.OnNext(message);
            }

            drained.Clear();

            // 溢れは詰まっている印。毎フレーム出すと洪水になるので1回だけ
            if (!didOverflow || loggedOverflow) return;
            loggedOverflow = true;
            RectorLogger.OscInputOverflow();
        }

        public void Dispose()
        {
            Close();
            disposable.Dispose();
            isListening.Dispose();
            messages.Dispose();
        }
    }
}
