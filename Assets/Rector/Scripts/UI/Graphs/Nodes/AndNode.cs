using R3;
using Rector.UI.Graphs.Slots;

namespace Rector.UI.Graphs.Nodes
{
    public sealed class AndNode : Node
    {
        public const string NodeName = "And";
        public static NodeCategory GetCategory() => NodeCategory.Operator;
        public override NodeCategory Category => GetCategory();

        // どちらかに値が届くたびに評価し直す。同じ値でも捨てない（BehaviorSubject）
        readonly BehaviorSubject<bool> x = new(false);
        readonly BehaviorSubject<bool> y = new(false);

        public AndNode(NodeId id) : base(id, NodeName)
        {
            InputSlots = new InputSlot[]
            {
                new BehaviorSubjectInputSlot<bool>(id, 0, "x", x, false, IsMuted),
                new BehaviorSubjectInputSlot<bool>(id, 1, "y", y, false, IsMuted),
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
