using R3;
using Rector.Audio;
using Rector.UI.Graphs.Slots;

namespace Rector.UI.Graphs.Nodes
{
    public sealed class LevelNode : SourceNode
    {
        public const string NodeName = "Level";
        public static string Category => NodeCategory.Event;

        public LevelNode(NodeId id, AudioMixerModel audioMixerModel) : base(id, NodeName)
        {
            InputSlots = new[]
            {
                SlotConverter.Convert(id, 0, ActiveInput, IsMuted),
            };
            OutputSlots = new OutputSlot[]
            {
                new ObservableOutputSlot<float>(id, 0, "Low", audioMixerModel.LevelLow.Where(_ => IsActive), IsMuted),
                new ObservableOutputSlot<float>(id, 1, "Mid", audioMixerModel.LevelMid.Where(_ => IsActive), IsMuted),
                new ObservableOutputSlot<float>(id, 2, "High", audioMixerModel.LevelHigh.Where(_ => IsActive), IsMuted)
            };
        }

        public override InputSlot[] InputSlots { get; }
        public override OutputSlot[] OutputSlots { get; }
    }
}
