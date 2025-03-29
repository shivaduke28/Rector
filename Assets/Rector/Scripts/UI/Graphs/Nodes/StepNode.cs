using R3;
using Rector.UI.Graphs.Slots;

namespace Rector.UI.Graphs.Nodes
{
    public sealed class StepNode : Node
    {
        public const string NodeName = "Step";
        public static string Category => NodeCategory.Math;
        readonly ReactiveProperty<float> x = new(0);
        readonly ReactiveProperty<float> a = new(1);

        public StepNode(NodeId id) : base(id, NodeName)
        {
            InputSlots = new InputSlot[]
            {
                new ReactivePropertyFloatInputSlot(id, 0, "x", x, 0f, float.NegativeInfinity, float.PositiveInfinity, IsMuted),
                new ReactivePropertyFloatInputSlot(id, 1, "a", a, 1f, float.NegativeInfinity, float.PositiveInfinity, IsMuted),
            };
            OutputSlots = new OutputSlot[]
            {
                new ObservableOutputSlot<float>(id, 0, "step", x.Select(v => v > a.Value ? 1f : 0f), IsMuted),
                new ObservableOutputSlot<float>(id, 1, "step*x", x.Select(v => v > a.Value ? v : 0f), IsMuted),
            };
        }

        public override InputSlot[] InputSlots { get; }
        public override OutputSlot[] OutputSlots { get; }
    }
}
