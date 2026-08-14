using R3;
using Rector.UI.Graphs.Slots;

namespace Rector.UI.Graphs.Nodes
{
    // Branch N: 1始まりの Index を排他的な bool に解釈する分岐ノード。
    // 0 は「何も選ばれていない」(全出力オフ)、k >= 1 は出力 "1 + (k - 1) % N" だけ点灯。
    // Seq.Beat や Loop.Phase/Cycle の1始まりの位置をそのまま受けられる。
    public sealed class Branch2Node : Node
    {
        public const string NodeName = "Branch 2";
        public static NodeCategory GetCategory() => NodeCategory.Operator;
        public override NodeCategory Category => GetCategory();
        public override InputSlot[] InputSlots { get; }
        public override OutputSlot[] OutputSlots { get; }

        readonly ReactiveProperty<int> index = new(0);

        public Branch2Node(NodeId id) : base(id, NodeName)
        {
            InputSlots = new InputSlot[]
            {
                new ReactivePropertyIntInputSlot(id, 0, "Index", index, 0, 0, 256, IsMuted)
            };

            OutputSlots = new OutputSlot[2];
            for (var i = 0; i < 2; i++)
            {
                var ind = i;
                OutputSlots[i] = new ObservableOutputSlot<bool>(id, i, (i + 1).ToString(),
                    index.Select(x => x >= 1 && (x - 1) % 2 == ind).DistinctUntilChanged(), IsMuted);
            }
        }
    }

    public sealed class Branch4Node : Node
    {
        public const string NodeName = "Branch 4";
        public static NodeCategory GetCategory() => NodeCategory.Operator;
        public override NodeCategory Category => GetCategory();
        public override InputSlot[] InputSlots { get; }
        public override OutputSlot[] OutputSlots { get; }

        readonly ReactiveProperty<int> index = new(0);

        public Branch4Node(NodeId id) : base(id, NodeName)
        {
            InputSlots = new InputSlot[]
            {
                new ReactivePropertyIntInputSlot(id, 0, "Index", index, 0, 0, 256, IsMuted)
            };

            OutputSlots = new OutputSlot[4];
            for (var i = 0; i < 4; i++)
            {
                var ind = i;
                OutputSlots[i] = new ObservableOutputSlot<bool>(id, i, (i + 1).ToString(),
                    index.Select(x => x >= 1 && (x - 1) % 4 == ind).DistinctUntilChanged(), IsMuted);
            }
        }
    }

    public sealed class Branch16Node : Node
    {
        public const string NodeName = "Branch 16";
        public static NodeCategory GetCategory() => NodeCategory.Operator;
        public override NodeCategory Category => GetCategory();
        public override InputSlot[] InputSlots { get; }
        public override OutputSlot[] OutputSlots { get; }

        readonly ReactiveProperty<int> index = new(0);

        public Branch16Node(NodeId id) : base(id, NodeName)
        {
            InputSlots = new InputSlot[]
            {
                new ReactivePropertyIntInputSlot(id, 0, "Index", index, 0, 0, 256, IsMuted)
            };

            OutputSlots = new OutputSlot[16];
            for (var i = 0; i < 16; i++)
            {
                var ind = i;
                OutputSlots[i] = new ObservableOutputSlot<bool>(id, i, (i + 1).ToString(),
                    index.Select(x => x >= 1 && (x - 1) % 16 == ind).DistinctUntilChanged(), IsMuted);
            }
        }
    }
}
