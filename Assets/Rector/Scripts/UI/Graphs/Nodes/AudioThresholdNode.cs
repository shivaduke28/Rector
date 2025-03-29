using R3;
using Rector.Audio;
using Rector.UI.Graphs.Slots;

namespace Rector.UI.Graphs.Nodes
{
    public sealed class AudioThresholdNode : SourceNode
    {
        public const string NodeName = "Audio Thld";
        public static string Category => NodeCategoryV2.Event;
        public AudioThresholdNode(NodeId id, AudioMixerModel audioMixerModel) : base(id, NodeName)
        {
            InputSlots = new InputSlot[]
            {
                new ReactivePropertyInputSlot<bool>(Id, 0, "Active", IsActive, IsActive.Value, IsMuted)
            };
            OutputSlots = new OutputSlot[]
            {
                new ObservableOutputSlot<bool>(Id, 0, "low",
                    audioMixerModel.LevelLow.Select(x => x >= audioMixerModel.ThLow.Value).Where(_ => IsActive.Value).DistinctUntilChanged(), IsMuted),
                new ObservableOutputSlot<bool>(Id, 1, "mid",
                    audioMixerModel.LevelMid.Select(x => x >= audioMixerModel.ThMid.Value).Where(_ => IsActive.Value).DistinctUntilChanged(), IsMuted),
                new ObservableOutputSlot<bool>(Id, 2, "high",
                    audioMixerModel.LevelHigh.Select(x => x >= audioMixerModel.ThHigh.Value).Where(_ => IsActive.Value).DistinctUntilChanged(), IsMuted)
            };
        }

        public override InputSlot[] InputSlots { get; }
        public override OutputSlot[] OutputSlots { get; }
    }
}
