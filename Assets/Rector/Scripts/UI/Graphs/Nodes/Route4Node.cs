using R3;
using Rector.UI.Graphs.Slots;

namespace Rector.UI.Graphs.Nodes
{
    // Route N: 1始まりの Index を N 本のレーンに振り分けるルーティングノード。
    // 選ばれたレーン "1 + (Index - 1) % N" には Index をそのまま流し、他のレーンには 0 を流す。
    // 0 は「無し」なので、下流の Int→Bool (x != 0) で点灯/消灯がそのまま伝わり、
    // Route の下に Route/Loop/Equal を重ねると位置が木を流れ落ちて排他が構造的に保たれる。
    public sealed class Route2Node : Node
    {
        public const string NodeName = "Route 2";
        public static NodeCategory GetCategory() => NodeCategory.Operator;
        public override NodeCategory Category => GetCategory();
        public override InputSlot[] InputSlots { get; }
        public override OutputSlot[] OutputSlots { get; }

        readonly ReactiveProperty<int> index = new(0);

        public Route2Node(NodeId id) : base(id, NodeName)
        {
            InputSlots = new InputSlot[]
            {
                new ReactivePropertyIntInputSlot(id, 0, "Index", index, 0, 0, 256, IsMuted)
            };

            OutputSlots = new OutputSlot[2];
            for (var i = 0; i < 2; i++)
            {
                var ind = i;
                OutputSlots[i] = new ObservableOutputSlot<int>(id, i, (i + 1).ToString(),
                    index.Select(x => x >= 1 && (x - 1) % 2 == ind ? x : 0).DistinctUntilChanged(), IsMuted);
            }
        }
    }

    public sealed class Route4Node : Node
    {
        public const string NodeName = "Route 4";
        public static NodeCategory GetCategory() => NodeCategory.Operator;
        public override NodeCategory Category => GetCategory();
        public override InputSlot[] InputSlots { get; }
        public override OutputSlot[] OutputSlots { get; }

        readonly ReactiveProperty<int> index = new(0);

        public Route4Node(NodeId id) : base(id, NodeName)
        {
            InputSlots = new InputSlot[]
            {
                new ReactivePropertyIntInputSlot(id, 0, "Index", index, 0, 0, 256, IsMuted)
            };

            OutputSlots = new OutputSlot[4];
            for (var i = 0; i < 4; i++)
            {
                var ind = i;
                OutputSlots[i] = new ObservableOutputSlot<int>(id, i, (i + 1).ToString(),
                    index.Select(x => x >= 1 && (x - 1) % 4 == ind ? x : 0).DistinctUntilChanged(), IsMuted);
            }
        }
    }

    public sealed class Route16Node : Node
    {
        public const string NodeName = "Route 16";
        public static NodeCategory GetCategory() => NodeCategory.Operator;
        public override NodeCategory Category => GetCategory();
        public override InputSlot[] InputSlots { get; }
        public override OutputSlot[] OutputSlots { get; }

        readonly ReactiveProperty<int> index = new(0);

        public Route16Node(NodeId id) : base(id, NodeName)
        {
            InputSlots = new InputSlot[]
            {
                new ReactivePropertyIntInputSlot(id, 0, "Index", index, 0, 0, 256, IsMuted)
            };

            OutputSlots = new OutputSlot[16];
            for (var i = 0; i < 16; i++)
            {
                var ind = i;
                OutputSlots[i] = new ObservableOutputSlot<int>(id, i, (i + 1).ToString(),
                    index.Select(x => x >= 1 && (x - 1) % 16 == ind ? x : 0).DistinctUntilChanged(), IsMuted);
            }
        }
    }
}
