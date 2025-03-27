using R3;
using Rector.Audio;
using Rector.UI.Graphs;

namespace Rector.UI.Nodes
{
    public sealed class LevelNode : SourceNode
    {
        public const string NodeName = "Level";
        public LevelNode(NodeId id, AudioMixerModel audioMixerModel) : base(id, NodeName)
        {
            InputSlots = new InputSlot[]
            {
                new ReactivePropertyInputSlot<bool>(Id, 0, "Active", IsActive, IsActive.Value, IsMuted)
            };
            OutputSlots = new OutputSlot[]
            {
                new ObservableOutputSlot<float>(id, 0, "Low", audioMixerModel.LevelLow.Where(_ => IsActive.Value), IsMuted),
                new ObservableOutputSlot<float>(id, 1, "Mid", audioMixerModel.LevelMid.Where(_ => IsActive.Value), IsMuted),
                new ObservableOutputSlot<float>(id, 2, "High", audioMixerModel.LevelHigh.Where(_ => IsActive.Value), IsMuted)
            };
        }

        public override InputSlot[] InputSlots { get; }
        public override OutputSlot[] OutputSlots { get; }
    }
}
