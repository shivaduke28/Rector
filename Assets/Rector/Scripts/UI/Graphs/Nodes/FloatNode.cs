using Rector.Nodes;

namespace Rector.UI.Graphs.Nodes
{
    public sealed class FloatNode : Node
    {
        public const string NodeName = "Float";
        public static string Category => NodeCategoryV2.Math;

        public FloatNode(NodeId id) : base(id, NodeName)
        {
            var input = new FloatInput("In", 0, float.NegativeInfinity, float.PositiveInfinity);
            var output = new ObservableOutput<float>("Out", input.Value);

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
