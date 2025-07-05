using R3;
using Rector.NodeBehaviours;
using Rector.UI.Graphs.Slots;
using UnityEngine;

namespace Rector.UI.Graphs.Nodes
{
    public sealed class CircleNode : Node
    {
        public const string NodeName = "Circle";
        public static NodeCategory GetCategory() => NodeCategory.Math;
        public override NodeCategory Category => GetCategory();

        // normalized
        readonly FloatInput theta = new("t", 0, -1f, 1f);
        readonly FloatInput radius = new("r", 1f, 0, 100f);

        public CircleNode(NodeId id) : base(id, NodeName)
        {
            InputSlots = new[]
            {
                SlotConverter.Convert(id, 0, theta, IsMuted),
                SlotConverter.Convert(id, 1, radius, IsMuted)
            };

            OutputSlots = new OutputSlot[]
            {
                new ObservableOutputSlot<float>(id, 0, "x", theta.Value.CombineLatest(radius.Value, (t, r) => Mathf.Cos(t * Mathf.PI * 2f) * r), IsMuted),
                new ObservableOutputSlot<float>(id, 1, "y", theta.Value.CombineLatest(radius.Value, (t, r) => Mathf.Sin(t * Mathf.PI * 2f) * r), IsMuted)
            };
        }

        public override InputSlot[] InputSlots { get; }
        public override OutputSlot[] OutputSlots { get; }
    }
}
