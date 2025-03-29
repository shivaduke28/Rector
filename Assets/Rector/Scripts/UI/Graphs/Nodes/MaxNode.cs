using R3;
using Rector.Nodes;
using Rector.UI.Graphs.Slots;
using UnityEngine;

namespace Rector.UI.Graphs.Nodes
{
    public sealed class MaxNode : Node
    {
        public const string NodeName = "Max";
        public static string Category => NodeCategoryV2.Math;

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
        public static string Category => NodeCategoryV2.Math;

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

    // Mod Node
    public sealed class ModNode : Node
    {
        const float Min = 0.01f;
        public const string NodeName = "Mod";
        public static string Category => NodeCategoryV2.Math;

        readonly FloatInput x = new("x", 0f, float.NegativeInfinity, float.PositiveInfinity);
        readonly FloatInput y = new("y", 1f, Min, float.PositiveInfinity);

        public ModNode(NodeId id) : base(id, NodeName)
        {
            InputSlots = new[]
            {
                SlotConverter.Convert(id, 0, x, IsMuted),
                SlotConverter.Convert(id, 1, y, IsMuted)
            };

            OutputSlots = new[]
            {
                SlotConverter.Convert(id, 0,
                    new ObservableOutput<float>("Out", x.Value.CombineLatest(y.Value.Select(t => Mathf.Max(t, Min)), (x1, y1) => x1 % y1)), IsMuted)
            };
        }

        public override InputSlot[] InputSlots { get; }
        public override OutputSlot[] OutputSlots { get; }
    }
}
