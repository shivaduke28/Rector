using System;
using R3;
using Rector.NodeBehaviours;

namespace Rector.UI.Graphs.Nodes
{
    /// <summary>
    /// MIDI ソースノードの共通基底。ノート/CC 番号の保持と MIDI ラーン（Learn を押した後の
    /// 最初の MIDI 入力で番号を確定する）をここで面倒みる。
    /// </summary>
    public abstract class MidiSourceNode : SourceNode, IDisposable
    {
        public static NodeCategory GetCategory() => NodeCategory.Event;
        public override NodeCategory Category => GetCategory();

        protected readonly IntInput NumberInput;
        readonly Observable<int> learnSource;
        readonly ReactiveProperty<bool> isLearning = new(false);
        readonly SerialDisposable learnSubscription = new();

        public Observable<string> DisplayLabel { get; }

        protected MidiSourceNode(NodeId id, string name, IntInput numberInput, Observable<int> learnSource) : base(id, name)
        {
            NumberInput = numberInput;
            this.learnSource = learnSource;
            DisplayLabel = numberInput.Value.CombineLatest(isLearning, (number, learning) => learning ? $"{name} [LEARN]" : $"{name} {number}");
        }

        // ラーンはアサイン操作なので Active/Mute に関係なく生ストリームを拾う
        protected void ToggleLearn()
        {
            if (isLearning.Value)
            {
                learnSubscription.Disposable = null;
                isLearning.Value = false;
                RectorLogger.MidiLearn(this, "cancelled");
                return;
            }

            isLearning.Value = true;
            RectorLogger.MidiLearn(this, "armed");
            learnSubscription.Disposable = learnSource
                .Take(1)
                .Subscribe(number =>
                {
                    NumberInput.Value.Value = number;
                    isLearning.Value = false;
                    RectorLogger.MidiLearn(this, $"assigned {number}");
                });
        }

        public void Dispose()
        {
            learnSubscription.Dispose();
            isLearning.Dispose();
        }
    }
}
