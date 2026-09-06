using R3;
using Rector.UI.Graphs.Slots;

namespace Rector.UI.Graphs.Nodes
{
    public sealed class NegateNode : Node
    {
        public const string NodeName = "Negate";
        public static NodeCategory GetCategory() => NodeCategory.Operator;
        public override NodeCategory Category => GetCategory();

        // 同じ値が続けて届いても毎回出す（外れが2連続したときの2回目も下流へ届ける）ため、同値を捨てない BehaviorSubject で持つ
        readonly BehaviorSubject<bool> input = new(false);

        public NegateNode(NodeId id) : base(id, NodeName)
        {
            InputSlots = new InputSlot[]
            {
                new BehaviorSubjectInputSlot<bool>(id, 0, "In", input, false, IsMuted)
            };
            OutputSlots = new OutputSlot[]
            {
                new ObservableOutputSlot<bool>(id, 0, "Out", input.Select(x => !x), IsMuted)
            };
        }

        public override InputSlot[] InputSlots { get; }
        public override OutputSlot[] OutputSlots { get; }
    }
}
