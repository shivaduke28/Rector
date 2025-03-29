using R3;
using Rector.UI.Graphs.Slots;

namespace Rector.UI.Graphs.Nodes
{
    public sealed class UpdateNode : SourceNode
    {
        public const string NodeName = "Update";
        public static string Category => NodeCategoryV2.Event;

        public UpdateNode(NodeId id) : base(id, NodeName)
        {
            InputSlots = new InputSlot[]
            {
                new ReactivePropertyInputSlot<bool>(Id, 0, "Active", IsActive, IsActive.Value, IsMuted)
            };

            OutputSlots = new OutputSlot[]
            {
                new ObservableOutputSlot<Unit>(id, 0, "Update", Observable.EveryUpdate(UnityFrameProvider.Update).Where(_ => IsActive.Value).AsUnitObservable(), IsMuted)
            };
        }

        public override InputSlot[] InputSlots { get; }
        public override OutputSlot[] OutputSlots { get; }
    }
}
