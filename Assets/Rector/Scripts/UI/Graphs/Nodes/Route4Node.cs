using R3;
using Rector.UI.Graphs.Slots;

namespace Rector.UI.Graphs.Nodes
{
    // Route N: 1始まりの Index を N 本のレーンに振り分けるルーティングノード。
    // 選ばれたレーン "1 + (Index - 1) % N" には Index をそのまま流す。
    // Send 0 が ON (既定) のとき、レーンは脱選択の瞬間に 0 を1回流す。
    // 0 は「無し」の符号で、下流の Int→Bool (x != 0) を通じて VFX の消灯や
    // レーン点灯表示を担う。OFF にするとマッチした値だけが流れる純粋な選別になる。
    // Send 0 は emission ごとにサンプルされる (イベント出力ノードのパラメータ規則)。
    public sealed class Route2Node : Node
    {
        public const string NodeName = "Route 2";
        public static NodeCategory GetCategory() => NodeCategory.Operator;
        public override NodeCategory Category => GetCategory();
        public override InputSlot[] InputSlots { get; }
        public override OutputSlot[] OutputSlots { get; }

        readonly ReactiveProperty<int> index = new(0);
        readonly ReactiveProperty<bool> sendZero = new(true);

        public Route2Node(NodeId id) : base(id, NodeName)
        {
            InputSlots = new InputSlot[]
            {
                new ReactivePropertyIntInputSlot(id, 0, "Index", index, 0, 0, 256, IsMuted),
                new ReactivePropertyInputSlot<bool>(id, 1, "Send 0", sendZero, true, IsMuted)
            };

            OutputSlots = new OutputSlot[2];
            for (var i = 0; i < 2; i++)
            {
                var ind = i;
                OutputSlots[i] = new ObservableOutputSlot<int>(id, i, (i + 1).ToString(),
                    index.Select(x => x >= 1 && (x - 1) % 2 == ind ? x : 0)
                        .DistinctUntilChanged()
                        .Where(x => x != 0 || sendZero.Value), IsMuted);
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
        readonly ReactiveProperty<bool> sendZero = new(true);

        public Route4Node(NodeId id) : base(id, NodeName)
        {
            InputSlots = new InputSlot[]
            {
                new ReactivePropertyIntInputSlot(id, 0, "Index", index, 0, 0, 256, IsMuted),
                new ReactivePropertyInputSlot<bool>(id, 1, "Send 0", sendZero, true, IsMuted)
            };

            OutputSlots = new OutputSlot[4];
            for (var i = 0; i < 4; i++)
            {
                var ind = i;
                OutputSlots[i] = new ObservableOutputSlot<int>(id, i, (i + 1).ToString(),
                    index.Select(x => x >= 1 && (x - 1) % 4 == ind ? x : 0)
                        .DistinctUntilChanged()
                        .Where(x => x != 0 || sendZero.Value), IsMuted);
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
        readonly ReactiveProperty<bool> sendZero = new(true);

        public Route16Node(NodeId id) : base(id, NodeName)
        {
            InputSlots = new InputSlot[]
            {
                new ReactivePropertyIntInputSlot(id, 0, "Index", index, 0, 0, 256, IsMuted),
                new ReactivePropertyInputSlot<bool>(id, 1, "Send 0", sendZero, true, IsMuted)
            };

            OutputSlots = new OutputSlot[16];
            for (var i = 0; i < 16; i++)
            {
                var ind = i;
                OutputSlots[i] = new ObservableOutputSlot<int>(id, i, (i + 1).ToString(),
                    index.Select(x => x >= 1 && (x - 1) % 16 == ind ? x : 0)
                        .DistinctUntilChanged()
                        .Where(x => x != 0 || sendZero.Value), IsMuted);
            }
        }
    }
}
