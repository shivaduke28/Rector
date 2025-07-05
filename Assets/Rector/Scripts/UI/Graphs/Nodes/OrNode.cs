using R3;
using Rector.UI.Graphs.Slots;

namespace Rector.UI.Graphs.Nodes
{
    public sealed class OrNode : Node
    {
        public const string NodeName = "Or";
        public static string Category => NodeCategory.Operator;
        readonly ReactiveProperty<bool> x = new(true);
        readonly ReactiveProperty<bool> y = new(true);

        public OrNode(NodeId id) : base(id, NodeName)
        {
            InputSlots = new InputSlot[]
            {
                new ReactivePropertyInputSlot<bool>(id, 0, "x", x, x.Value, IsMuted),
                new ReactivePropertyInputSlot<bool>(id, 1, "y", y, y.Value, IsMuted),
            };

            OutputSlots = new OutputSlot[]
            {
                new ObservableOutputSlot<bool>(id, 0, "Out", x.CombineLatest(y, (a, b) => a || b), IsMuted)
            };
        }

        public override InputSlot[] InputSlots { get; }
        public override OutputSlot[] OutputSlots { get; }
    }
}
