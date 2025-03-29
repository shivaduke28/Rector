using R3;
using Rector.Nodes;
using Rector.UI.Graphs.Slots;
using UnityEngine;

namespace Rector.UI.Graphs.Nodes
{
    public sealed class SinNode : Node
    {
        public const string NodeName = "Sin";
        public static string Category => NodeCategoryV2.Math;

        public SinNode(NodeId id) : base(id, NodeName)
        {
            var input = new FloatInput("In", 0, float.NegativeInfinity, float.PositiveInfinity);
            var output = new ObservableOutput<float>("Out", input.Value.Select(Mathf.Sin));

            InputSlots = new[]
            {
                SlotConverter.Convert(id, 0, input, IsMuted)
            };

            OutputSlots = new[]
            {
                SlotConverter.Convert(id, 0, output, IsMuted)
            };
        }


        public override InputSlot[] InputSlots { get; }
        public override OutputSlot[] OutputSlots { get; }
    }

    public sealed class CosNode : Node
    {
        public const string NodeName = "Cos";

        public CosNode(NodeId id) : base(id, NodeName)
        {
            var input = new FloatInput("In", 0, float.NegativeInfinity, float.PositiveInfinity);
            var output = new ObservableOutput<float>("Out", input.Value.Select(Mathf.Cos));

            InputSlots = new[]
            {
                SlotConverter.Convert(id, 0, input, IsMuted)
            };

            OutputSlots = new[]
            {
                SlotConverter.Convert(id, 0, output, IsMuted)
            };
        }

        public override InputSlot[] InputSlots { get; }
        public override OutputSlot[] OutputSlots { get; }
    }
}
