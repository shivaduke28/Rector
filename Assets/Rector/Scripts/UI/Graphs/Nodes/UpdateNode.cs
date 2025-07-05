using R3;
using Rector.UI.Graphs.Slots;

namespace Rector.UI.Graphs.Nodes
{
    public sealed class UpdateNode : SourceNode
    {
        public const string NodeName = "Update";
        public static string Category => NodeCategory.Event;

        public UpdateNode(NodeId id) : base(id, NodeName)
        {
            InputSlots = new InputSlot[]
            {
                SlotConverter.Convert(id, 0, ActiveInput, IsMuted),
            };

            OutputSlots = new OutputSlot[]
            {
                new ObservableOutputSlot<Unit>(id, 0, "Update", Observable.EveryUpdate(UnityFrameProvider.Update).Where(_ => IsActive).AsUnitObservable(), IsMuted)
            };
        }

        public override InputSlot[] InputSlots { get; }
        public override OutputSlot[] OutputSlots { get; }
    }
}
