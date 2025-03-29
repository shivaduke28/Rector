using R3;
using Rector.Nodes;
using UnityEngine;

namespace Rector.UI.Graphs.Nodes
{
    public sealed class Vector3Node : Node
    {
        public const string NodeName = "Vector3";
        public static string Category => NodeCategoryV2.Math;

        public Vector3Node(NodeId id) : base(id, NodeName)
        {
            // negative infinity, positive infinity
            var inputX = new FloatInput("X", 0, float.NegativeInfinity, float.PositiveInfinity);
            var inputY = new FloatInput("Y", 0, float.NegativeInfinity, float.PositiveInfinity);
            var inputZ = new FloatInput("Z", 0, float.NegativeInfinity, float.PositiveInfinity);
            var output = new ObservableOutput<Vector3>("Out", inputX.Value.CombineLatest(inputY.Value, inputZ.Value, (x, y, z) => new Vector3(x, y, z)));

            InputSlots = new[]
            {
                SlotConverter.Convert(id, 0, inputX, IsMuted),
                SlotConverter.Convert(id, 1, inputY, IsMuted),
                SlotConverter.Convert(id, 2, inputZ, IsMuted)
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
