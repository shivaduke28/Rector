using R3;
using Rector.UI.Graphs.Slots;

namespace Rector.UI.Graphs.Nodes
{
    // Branch N: Index の int を排他的な bool に解釈する分岐ノード。
    // 0 は「何も選ばれていない」(全出力オフ)、k >= 1 は out[(k - 1) % N] だけ点灯。
    // Switch 系の「選択中 = 1始まりの周回数, 非選択 = 0」の出力をそのまま受けられる。
    // Loop が 0 を正当な位置として扱うのに対し、Branch は 0 をオフ符号として扱う。
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
                OutputSlots[i] = new ObservableOutputSlot<bool>(id, i, i.ToString(),
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
                OutputSlots[i] = new ObservableOutputSlot<bool>(id, i, i.ToString(),
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
                OutputSlots[i] = new ObservableOutputSlot<bool>(id, i, i.ToString(),
                    index.Select(x => x >= 1 && (x - 1) % 16 == ind).DistinctUntilChanged(), IsMuted);
            }
        }
    }
}
