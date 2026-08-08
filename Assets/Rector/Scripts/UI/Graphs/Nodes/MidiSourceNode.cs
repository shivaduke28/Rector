using System;
using R3;
using Rector.NodeBehaviours;

namespace Rector.UI.Graphs.Nodes
{
    /// <summary>
    /// MIDI ソースノードの共通基底。ノート/CC 番号の保持と MIDI ラーン
    /// （Learn トグルを on にすると、次の MIDI 入力で番号を確定して自動で off に戻る）をここで面倒みる。
    /// </summary>
    public abstract class MidiSourceNode : SourceNode, IDisposable
    {
        public static NodeCategory GetCategory() => NodeCategory.MIDI;
        public override NodeCategory Category => GetCategory();

        protected readonly IntInput NumberInput;
        protected readonly BoolInput LearnInput;
        readonly SerialDisposable learnSubscription = new();
        readonly IDisposable learnStateSubscription;

        public Observable<string> DisplayLabel { get; }

        // ゲージ表示用の [0,1] 値。番号一致でフィルタ済み・Active/Mute は無視（入力が来ていること自体の可視化）
        public Observable<float> DisplayValue { get; protected set; }

        protected MidiSourceNode(NodeId id, string name, IntInput numberInput, Observable<int> learnSource) : base(id, name)
        {
            NumberInput = numberInput;
            LearnInput = new BoolInput("Learn", false);
            DisplayLabel = numberInput.Value.Select(number => $"{name} {number}");

            // ラーンはアサイン操作なので Active/Mute に関係なく生ストリームを拾う
            learnStateSubscription = LearnInput.Value.Subscribe(armed =>
            {
                if (armed)
                {
                    RectorLogger.MidiLearn(this, "armed");
                    learnSubscription.Disposable = learnSource
                        .Take(1)
                        .Subscribe(number =>
                        {
                            NumberInput.Value.Value = number;
                            LearnInput.Value.Value = false;
                            RectorLogger.MidiLearn(this, $"assigned {number}");
                        });
                }
                else
                {
                    learnSubscription.Disposable = null;
                }
            });
        }

        public void Dispose()
        {
            learnStateSubscription.Dispose();
            learnSubscription.Dispose();
        }
    }
}
