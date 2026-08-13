using R3;
using Rector.UI.Graphs.Slots;

namespace Rector.UI.Graphs.Nodes
{
    // Switch 4/16/4x4 の出力は「選択中 = 1始まりの周回数, 非選択 = 0」の int。
    // Int→Bool 変換は x != 0、Int→Unit は x != 0 で発火なので、
    // 従来の bool 出力（true/false）と同じ感覚で VFX の on/off やイベント駆動に使える。
    // 周回数が 0 だと選択中なのに falsy になるため、必ず 1 始まりにすること。
    public sealed class Switch4Node : Node
    {
        public const string NodeName = "Switch 4";
        public static NodeCategory GetCategory() => NodeCategory.Operator;
        public override NodeCategory Category => GetCategory();
        readonly ReactiveProperty<int> count = new(0);

        public Switch4Node(NodeId id) : base(id, NodeName)
        {
            InputSlots = new InputSlot[]
            {
                new CallbackInputSlot(id, 0, "In", Step, IsMuted)
            };

            OutputSlots = new OutputSlot[4];
            for (var i = 0; i < 4; i++)
            {
                var ind = i;
                OutputSlots[i] = new ObservableOutputSlot<int>(id, i, i.ToString(),
                    count.Select(x => x % 4 == ind ? x / 4 + 1 : 0).DistinctUntilChanged(), IsMuted);
            }
        }

        void Step()
        {
            count.Value++;
        }

        public override void DoAction() => Step();

        public override InputSlot[] InputSlots { get; }
        public override OutputSlot[] OutputSlots { get; }
    }

    public sealed class Switch2Node : Node
    {
        public const string NodeName = "Switch 2";
        public static NodeCategory GetCategory() => NodeCategory.Operator;
        public override NodeCategory Category => GetCategory();

        readonly ReactiveProperty<bool> state = new(true);

        public Switch2Node(NodeId id) : base(id, NodeName)
        {
            InputSlots = new InputSlot[]
            {
                new CallbackInputSlot(id, 0, "In", () => state.Value = !state.Value, IsMuted)
            };

            OutputSlots = new OutputSlot[]
            {
                new ObservableOutputSlot<bool>(id, 0, "True", state.Select(x => x).DistinctUntilChanged(), IsMuted),
                new ObservableOutputSlot<bool>(id, 1, "False", state.Select(x => !x).DistinctUntilChanged(), IsMuted)
            };
        }

        public override void DoAction() => state.Value = !state.Value;

        public override InputSlot[] InputSlots { get; }
        public override OutputSlot[] OutputSlots { get; }
    }

    public sealed class Switch16Node : Node
    {
        public const string NodeName = "Switch 16";
        public static NodeCategory GetCategory() => NodeCategory.Operator;
        public override NodeCategory Category => GetCategory();

        readonly ReactiveProperty<int> count = new(0);
        public override InputSlot[] InputSlots { get; }
        public override OutputSlot[] OutputSlots { get; }

        public Switch16Node(NodeId id) : base(id, NodeName)
        {
            InputSlots = new InputSlot[]
            {
                new CallbackInputSlot(id, 0, "+1", () => Step(1), IsMuted),
                new CallbackInputSlot(id, 1, "+4", () => Step(4), IsMuted),
            };

            OutputSlots = new OutputSlot[16];
            for (var i = 0; i < 16; i++)
            {
                var ind = i;
                OutputSlots[i] = new ObservableOutputSlot<int>(id, i, i.ToString(),
                    count.Select(x => x % 16 == ind ? x / 16 + 1 : 0).DistinctUntilChanged(), IsMuted);
            }
        }

        void Step(int step)
        {
            count.Value += step;
        }

        public override void DoAction() => Step(1);
    }

    public sealed class Switch4By4Node : Node
    {
        public const string NodeName = "Switch 4x4";
        public static NodeCategory GetCategory() => NodeCategory.Operator;
        public override NodeCategory Category => GetCategory();
        readonly ReactiveProperty<int> count = new(0);
        public override InputSlot[] InputSlots { get; }
        public override OutputSlot[] OutputSlots { get; }

        public Switch4By4Node(NodeId id) : base(id, NodeName)
        {
            InputSlots = new InputSlot[]
            {
                new CallbackInputSlot(id, 0, "+1", () => Step(1), IsMuted),
                new CallbackInputSlot(id, 1, "+4", () => Step(4), IsMuted),
            };

            OutputSlots = new OutputSlot[4];
            for (var i = 0; i < 4; i++)
            {
                var ind = i;
                OutputSlots[i] = new ObservableOutputSlot<int>(id, i, i.ToString(),
                    count.Select(x => x % 16 / 4 == ind ? x / 16 + 1 : 0).DistinctUntilChanged(), IsMuted);
            }
        }

        void Step(int step)
        {
            count.Value += step;
        }

        public override void DoAction() => Step(4);
    }
}
