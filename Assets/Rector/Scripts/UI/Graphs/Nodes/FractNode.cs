using R3;
using Rector.NodeBehaviours;
using Rector.UI.Graphs.Slots;
using UnityEngine;

namespace Rector.UI.Graphs.Nodes
{
    public sealed class FractNode : Node
    {
        const float Min = 0.01f;
        public const string NodeName = "Fract";
        public static NodeCategory GetCategory() => NodeCategory.Math;
        public override NodeCategory Category => GetCategory();

        readonly FloatInput x = new("x", 0f, float.NegativeInfinity, float.PositiveInfinity);
        readonly FloatInput y = new("y", 1f, Min, float.PositiveInfinity);

        public FractNode(NodeId id) : base(id, NodeName)
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