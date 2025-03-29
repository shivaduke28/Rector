using R3;

namespace Rector.UI.Graphs.Nodes
{
    public sealed class AndNode : Node
    {
        public const string NodeName = "And";
        public static string Category => NodeCategoryV2.Operator;

        readonly ReactiveProperty<bool> x = new(false);
        readonly ReactiveProperty<bool> y = new(false);

        public AndNode(NodeId id) : base(id, NodeName)
        {
            InputSlots = new InputSlot[]
            {
                new ReactivePropertyInputSlot<bool>(id, 0, "x", x, x.Value, IsMuted),
                new ReactivePropertyInputSlot<bool>(id, 1, "y", y, y.Value, IsMuted),
            };

            OutputSlots = new OutputSlot[]
            {
                new ObservableOutputSlot<bool>(id, 0, "Out", x.CombineLatest(y, (a, b) => a && b), IsMuted)
            };
        }

        public override InputSlot[] InputSlots { get; }
        public override OutputSlot[] OutputSlots { get; }
    }
}
