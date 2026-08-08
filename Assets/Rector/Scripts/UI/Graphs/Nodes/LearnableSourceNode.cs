using System;
using R3;
using Rector.NodeBehaviours;

namespace Rector.UI.Graphs.Nodes
{
    /// <summary>
    /// 外部入力を1つ選んで受けるソースノードの共通基底。MIDI と OSC で共有する。
    /// ラーン（Learn を on にすると次の入力で対象を確定し、自動で off に戻る）をここで面倒みる。
    /// </summary>
    public abstract class LearnableSourceNode : SourceNode, IDisposable
    {
        public static NodeCategory GetCategory() => NodeCategory.Input;
        public override NodeCategory Category => GetCategory();

        protected readonly BoolInput LearnInput;
        readonly SerialDisposable learnSubscription = new();
        readonly IDisposable learnStateSubscription;

        /// <summary>ノード名の代わりに出す文字列。派生が自分の状態から組み立てる。</summary>
        public Observable<string> DisplayLabel { get; protected set; }

        // ゲージ表示用の [0,1] 値。対象一致でフィルタ済み・Active/Mute は無視（入力が来ていること自体の可視化）
        public Observable<float> DisplayValue { get; protected set; }

        protected LearnableSourceNode(NodeId id, string name) : base(id, name)
        {
            LearnInput = new BoolInput("Learn", false);

            // ラーンはアサイン操作なので Active/Mute に関係なく生ストリームを拾う。
            // LearnInput は初期値 false なのでこの Subscribe は即座に else 側で発火する。
            // つまりコンストラクタから SubscribeLearn() が呼ばれることはなく、
            // 派生のフィールドが未初期化のまま触られる心配はない
            learnStateSubscription = LearnInput.Value.Subscribe(armed =>
            {
                if (armed)
                {
                    RectorLogger.SourceNodeLearn(this, "armed");
                    learnSubscription.Disposable = SubscribeLearn();
                }
                else
                {
                    learnSubscription.Disposable = null;
                }
            });
        }

        /// <summary>
        /// Learn を on にしたときだけ呼ばれる。次の入力を1つだけ待って対象を確定し、Disarm を呼ぶこと。
        /// </summary>
        protected abstract IDisposable SubscribeLearn();

        /// <remarks>
        /// SubscribeLearn が張った購読の OnNext の中から呼ばれる。LearnInput を false にすると
        /// 上の Subscribe が走って learnSubscription が自分自身を破棄する形になるが、
        /// SerialDisposable がこれを吸収する。この再入は成立に効いているので崩さないこと。
        /// </remarks>
        protected void Disarm(string assigned)
        {
            LearnInput.Value.Value = false;
            RectorLogger.SourceNodeLearn(this, $"assigned {assigned}");
        }

        public void Dispose()
        {
            learnStateSubscription.Dispose();
            learnSubscription.Dispose();
        }
    }
}
