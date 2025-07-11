using R3;
using Rector.NodeBehaviours;
using Rector.UI.Graphs.Slots;
using UnityEngine;

namespace Rector.UI.Graphs.Nodes
{
    public sealed class MaxNode : Node
    {
        public const string NodeName = "Max";
        public static NodeCategory GetCategory() => NodeCategory.Math;
        public override NodeCategory Category => GetCategory();

        readonly FloatInput x = new("x", 0f, float.NegativeInfinity, float.PositiveInfinity);
        readonly FloatInput y = new("y", 0f, float.NegativeInfinity, float.PositiveInfinity);

        public MaxNode(NodeId id) : base(id, NodeName)
        {
            InputSlots = new[]
            {
                SlotConverter.Convert(id, 0, x, IsMuted),
                SlotConverter.Convert(id, 1, y, IsMuted)
            };

            OutputSlots = new[]
            {
                SlotConverter.Convert(id, 0,
                    new ObservableOutput<float>("Out", x.Value.CombineLatest(y.Value, Mathf.Max)), IsMuted)
            };
        }

        public override InputSlot[] InputSlots { get; }
        public override OutputSlot[] OutputSlots { get; }
    }

    public sealed class MinNode : Node
    {
        public const string NodeName = "Min";
        public static NodeCategory GetCategory() => NodeCategory.Math;
        public override NodeCategory Category => GetCategory();

        readonly FloatInput x = new("x", 0f, float.NegativeInfinity, float.PositiveInfinity);
        readonly FloatInput y = new("y", 0f, float.NegativeInfinity, float.PositiveInfinity);

        public MinNode(NodeId id) : base(id, NodeName)
        {
            InputSlots = new[]
            {
                SlotConverter.Convert(id, 0, x, IsMuted),
                SlotConverter.Convert(id, 1, y, IsMuted)
            };

            OutputSlots = new[]
            {
                SlotConverter.Convert(id, 0,
                    new ObservableOutput<float>("Out", x.Value.CombineLatest(y.Value, Mathf.Min)), IsMuted)
            };
        }

        public override InputSlot[] InputSlots { get; }
        public override OutputSlot[] OutputSlots { get; }
    }
}
