using R3;
using Rector.UI.Graphs.Slots;

namespace Rector.UI.Graphs.Nodes
{
    public sealed class NegateNode : Node
    {
        public const string NodeName = "Negate";
        public static string Category => NodeCategory.Operator;
        readonly ReactiveProperty<bool> property = new(false);

        public NegateNode(NodeId id) : base(id, NodeName)
        {
            InputSlots = new InputSlot[]
            {
                new ReactivePropertyInputSlot<bool>(id, 0, "In", property, property.Value, IsMuted)
            };
            OutputSlots = new OutputSlot[]
            {
                new ObservableOutputSlot<bool>(id, 0, "Out", property.Select(x => !x), IsMuted)
            };
        }

        public override InputSlot[] InputSlots { get; }
        public override OutputSlot[] OutputSlots { get; }
    }
}
