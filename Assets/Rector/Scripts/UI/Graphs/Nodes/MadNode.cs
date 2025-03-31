using R3;
using Rector.NodeBehaviours;
using Rector.UI.Graphs.Slots;

namespace Rector.UI.Graphs.Nodes
{
    // multiply and add
    public class MadNode : Node
    {
        public const string NodeName = "MAD";
        public static string Category => NodeCategory.Math;
        readonly FloatInput x = new("x", 0f, float.NegativeInfinity, float.PositiveInfinity);
        readonly FloatInput a = new("a", 1f, float.NegativeInfinity, float.PositiveInfinity);
        readonly FloatInput b = new("b", 0f, float.NegativeInfinity, float.PositiveInfinity);

        public MadNode(NodeId id) : base(id, NodeName)
        {
            InputSlots = new[]
            {
                SlotConverter.Convert(id, 0, x, IsMuted),
                SlotConverter.Convert(id, 1, a, IsMuted),
                SlotConverter.Convert(id, 2, b, IsMuted),
            };

            OutputSlots = new[]
            {
                SlotConverter.Convert(id, 0,
                    new ObservableOutput<float>("Out", a.Value.CombineLatest(x.Value, b.Value, (a1, x1, b1) => a1 * x1 + b1)), IsMuted)
            };
        }

        public override InputSlot[] InputSlots { get; }
        public override OutputSlot[] OutputSlots { get; }
    }
}
