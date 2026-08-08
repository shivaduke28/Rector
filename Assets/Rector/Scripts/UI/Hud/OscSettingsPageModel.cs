using System;
using System.Collections.Generic;
using System.Linq;
using R3;
using Rector.Osc;
using Rector.UI.Settings;

namespace Rector.UI.Hud
{
    public sealed class OscSettingsPageModel : IInitializable, IDisposable, ISettingsPageModel
    {
        // 左がOff、右がOn。MIDI入力デバイスのページと同じ見せ方に揃える
        static readonly string[] SwitchOptions = { "Off", "On" };

        // 値列に1行で収まる上限。有線とWi-Fiが同時に上がっていれば2つ出る
        const int MaxShownAddresses = 2;

        readonly OscInputSetting setting;
        readonly OscModel oscModel;
        readonly SettingsPageView view;
        readonly ReactiveProperty<bool> isVisible = new(false);

        readonly StepperRowState receiveRow;

        // ポートを送るたびにソケットを張り直すことになるので、確定するまで適用しない行にする
        readonly SelectorRowState portRow;

        // 送信側に打ち込む宛先。設定ではなく設定の結果なので操作は受け付けない
        readonly TextRowState addressRow;

        readonly ISettingRow[] rows;
        readonly CompositeDisposable disposable = new();

        Action onExit;

        ReadOnlyReactiveProperty<bool> ISettingsPageModel.IsVisible => isVisible;
        IReadOnlyList<ISettingRow> ISettingsPageModel.GetRows() => rows;

        public OscSettingsPageModel(OscInputSetting setting, OscModel oscModel, SettingsPageView view)
        {
            this.setting = setting;
            this.oscModel = oscModel;
            this.view = view;

            receiveRow = new StepperRowState("Receive", SwitchOptions, 1, i => setting.SetEnabled(i == 1));
            portRow = new SelectorRowState("Port", i => setting.SetPort(OscInputSetting.PortCandidates[i]));
            addressRow = new TextRowState("Send To");
            rows = new ISettingRow[] { receiveRow, portRow, addressRow };
        }

        void IInitializable.Initialize()
        {
            view.Bind(this).AddTo(disposable);

            // 設定は行の外からも変わる(ポート衝突で受信が落ちるなど)。
            // 開いている最中に実態が動いても表示が嘘にならないよう購読しておく
            setting.Config.Subscribe(_ => Refresh()).AddTo(disposable);

            // 受信状態で変わるのは宛先の行だけ。Refresh を丸ごと呼ぶと、
            // 入切のたびにNICの列挙とポートメニューの作り直しが二重に走る
            oscModel.IsListening.Subscribe(_ => RefreshAddress()).AddTo(disposable);
        }

        public void Enter(Action onExitAction)
        {
            Refresh();
            onExit = onExitAction;
            isVisible.Value = true;
        }

        void Refresh()
        {
            var config = setting.Config.CurrentValue;

            receiveRow.SetIndexWithoutNotify(config.Enabled ? 1 : 0);

            // SelectorRowState は候補を自分で持たない。流し込まないと行が無反応になる
            portRow.SetOptions(
                OscInputSetting.PortCandidates.Select(x => x.ToString()).ToArray(),
                Array.IndexOf(OscInputSetting.PortCandidates, config.Port));

            RefreshAddress();
        }

        void RefreshAddress() => addressRow.SetText(BuildAddressText(setting.Config.CurrentValue));

        string BuildAddressText(OscInputConfig config)
        {
            if (!config.Enabled) return "Off";

            // 有効なのに開けていないのは、ほぼポートの取り合い。
            // ここで宛先を見せると届かないアドレスを打たせることになる
            if (!oscModel.IsListening.CurrentValue) return $"port {config.Port} unavailable";

            var addresses = OscLocalAddress.GetIPv4Addresses();
            if (addresses.Length == 0) return $"127.0.0.1:{config.Port}";

            // 値列は1行で、溢れると末尾が省略記号に落ちる。宛先を読ませる行なのに
            // 途中で切れては意味が無いので、並べるのは2つまでにして残りは数で示す。
            // 全部は起動ログに出ているのでそちらで読める
            var shown = string.Join(", ", addresses.Take(MaxShownAddresses).Select(a => $"{a}:{config.Port}"));
            return addresses.Length > MaxShownAddresses
                ? $"{shown} +{addresses.Length - MaxShownAddresses}"
                : shown;
        }

        void ISettingsPageModel.Cancel()
        {
            isVisible.Value = false;
            onExit?.Invoke();
            onExit = null;
        }

        void IDisposable.Dispose() => disposable.Dispose();
    }
}
