using R3;
using Rector.Audio;
using Rector.UI.Graphs.Slots;

namespace Rector.UI.Graphs.Nodes
{
    public sealed class BeatNode : SourceNode
    {
        public const string NodeName = "Beat";
        public static string NodeCategory => NodeCategoryV2.Event;
        public override InputSlot[] InputSlots { get; }
        public override OutputSlot[] OutputSlots { get; }
        readonly BeatModel beatModel;

        public ReadOnlyReactiveProperty<float> Bpm => beatModel.BpmProperty;

        public BeatNode(NodeId id, BeatModel beatModel) : base(id, NodeName)
        {
            this.beatModel = beatModel;
            InputSlots = new InputSlot[]
            {
                new ReactivePropertyInputSlot<bool>(Id, 0, "Active", IsActive, IsActive.Value, IsMuted),
                new CallbackInputSlot(Id, 1, "Tap", beatModel.Tap, IsMuted)
            };

            OutputSlots = new OutputSlot[]
            {
                new ObservableOutputSlot<Unit>(id, 0, "Beat", beatModel.BeatProperty.Where(_ => IsActive.Value).AsUnitObservable(), IsMuted),
                new ObservableOutputSlot<bool>(id, 1, "1", beatModel.BeatProperty.Where(_ => IsActive.Value).Select(x => x is 0).DistinctUntilChanged(), IsMuted),
                new ObservableOutputSlot<bool>(id, 2, "2", beatModel.BeatProperty.Where(_ => IsActive.Value).Select(x => x is 1).DistinctUntilChanged(), IsMuted),
                new ObservableOutputSlot<bool>(id, 3, "3", beatModel.BeatProperty.Where(_ => IsActive.Value).Select(x => x is 2).DistinctUntilChanged(), IsMuted),
                new ObservableOutputSlot<bool>(id, 4, "4", beatModel.BeatProperty.Where(_ => IsActive.Value).Select(x => x is 3).DistinctUntilChanged(), IsMuted)
            };
        }
    }
}
