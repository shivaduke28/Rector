using System;
using R3;
using Rector.NodeBehaviours;

namespace Rector.UI.Graphs.Nodes
{
    /// <summary>
    /// MIDI ソースノードの共通基底。ノート/CC 番号の保持と、そこへのラーンの割り当てを受け持つ。
    /// </summary>
    public abstract class MidiSourceNode : LearnableSourceNode
    {
        protected readonly IntInput NumberInput;
        readonly Observable<int> learnSource;

        protected MidiSourceNode(NodeId id, string name, IntInput numberInput, Observable<int> learnSource) : base(id, name)
        {
            NumberInput = numberInput;
            this.learnSource = learnSource;
            DisplayLabel = numberInput.Value.Select(number => $"{name} {number}");
        }

        protected override IDisposable SubscribeLearn()
            => learnSource
                .Take(1)
                .Subscribe(number =>
                {
                    NumberInput.Value.Value = number;
                    Disarm(number.ToString());
                });
    }
}
