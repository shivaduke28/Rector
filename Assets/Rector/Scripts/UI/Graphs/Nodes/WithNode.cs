using R3;

namespace Rector.UI.Graphs.Nodes
{
    public sealed class WithNode : Node
    {
        public const string NodeName = "With";
        public static string Category => NodeCategoryV2.Operator;
        readonly Subject<float> subject = new();
        readonly ReactiveProperty<float> x = new(0);

        public WithNode(NodeId id) : base(id, NodeName)
        {
            InputSlots = new InputSlot[]
            {
                new CallbackInputSlot(id, 0, "trigger", () => subject.OnNext(x.Value), IsMuted),
                new ReactivePropertyFloatInputSlot(id, 1, "value", x, 0, float.NegativeInfinity, float.PositiveInfinity, IsMuted),
            };
            OutputSlots = new OutputSlot[]
            {
                new ObservableOutputSlot<float>(id, 0, "value", subject, IsMuted),
            };
        }

        public override void DoAction()
        {
            subject.OnNext(x.Value);
        }

        public override InputSlot[] InputSlots { get; }
        public override OutputSlot[] OutputSlots { get; }
    }
}
