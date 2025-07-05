using R3;
using Rector.UI.Graphs.Slots;
using UnityEngine;

namespace Rector.UI.Graphs.Nodes
{
    public sealed class Switch4Node : Node
    {
        public const string NodeName = "Switch 4";
        public static NodeCategory GetCategory() => NodeCategory.Operator;
        public override NodeCategory Category => GetCategory();
        readonly ReactiveProperty<int> sequence = new(0);

        public Switch4Node(NodeId id) : base(id, NodeName)
        {
            InputSlots = new InputSlot[]
            {
                new CallbackInputSlot(id, 0, "In", Step, IsMuted)
            };

            OutputSlots = new OutputSlot[]
            {
                new ObservableOutputSlot<bool>(id, 0, "0", sequence.Select(x => x == 0).DistinctUntilChanged(), IsMuted),
                new ObservableOutputSlot<bool>(id, 1, "1", sequence.Select(x => x == 1).DistinctUntilChanged(), IsMuted),
                new ObservableOutputSlot<bool>(id, 2, "2", sequence.Select(x => x == 2).DistinctUntilChanged(), IsMuted),
                new ObservableOutputSlot<bool>(id, 3, "3", sequence.Select(x => x == 3).DistinctUntilChanged(), IsMuted),
            };
        }

        void Step()
        {
            sequence.Value = (sequence.Value + 1) % 4;
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

        readonly ReactiveProperty<int> sequence = new(0);
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
                OutputSlots[i] = new ObservableOutputSlot<bool>(id, i, i.ToString(),
                    sequence.Select(x => x == ind).DistinctUntilChanged(), IsMuted);
            }
        }

        void Step(int step)
        {
            sequence.Value = (sequence.Value + step) % 16;
        }

        public override void DoAction() => Step(1);
    }

    public sealed class Switch4By4Node : Node
    {
        public const string NodeName = "Switch 4x4";
        public static NodeCategory GetCategory() => NodeCategory.Operator;
        public override NodeCategory Category => GetCategory();
        readonly ReactiveProperty<int> sequence = new(0);
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
                OutputSlots[i] = new ObservableOutputSlot<bool>(id, i, i.ToString(),
                    sequence.Select(x => Mathf.FloorToInt(x / 4.0f) == ind).DistinctUntilChanged(), IsMuted);
            }
        }

        void Step(int step)
        {
            sequence.Value = (sequence.Value + step) % 16;
        }

        public override void DoAction() => Step(4);
    }
}
