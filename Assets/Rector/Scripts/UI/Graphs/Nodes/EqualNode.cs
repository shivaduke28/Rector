using R3;
using Rector.UI.Graphs.Slots;

namespace Rector.UI.Graphs.Nodes
{
    // Equal: 1始まりの Index を Value と比較する2択分岐。
    // 一致すれば Match に、一致しなければ NoMatch に Index をそのまま流す。
    // Index が 1 未満（=無し）のときは両方 0。Branch と同じ素通し規律なので木に重ねられる。
    public sealed class EqualNode : Node
    {
        public const string NodeName = "Equal";
        public static NodeCategory GetCategory() => NodeCategory.Operator;
        public override NodeCategory Category => GetCategory();
        public override InputSlot[] InputSlots { get; }
        public override OutputSlot[] OutputSlots { get; }

        readonly ReactiveProperty<int> index = new(0);
        readonly ReactiveProperty<int> value = new(1);

        public ReadOnlyReactiveProperty<int> Value => value;

        public EqualNode(NodeId id) : base(id, NodeName)
        {
            InputSlots = new InputSlot[]
            {
                new ReactivePropertyIntInputSlot(id, 0, "Index", index, 0, 0, 256, IsMuted),
                new ReactivePropertyIntInputSlot(id, 1, "Value", value, 1, 1, 256, IsMuted)
            };

            OutputSlots = new OutputSlot[]
            {
                new ObservableOutputSlot<int>(id, 0, "Match",
                    index.Select(x => x >= 1 && x == value.Value ? x : 0).DistinctUntilChanged(), IsMuted),
                new ObservableOutputSlot<int>(id, 1, "NoMatch",
                    index.Select(x => x >= 1 && x != value.Value ? x : 0).DistinctUntilChanged(), IsMuted)
            };
        }
    }
}
