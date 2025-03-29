using Rector.Nodes;
using Rector.UI.Graphs.Slots;

namespace Rector.UI.Graphs.Nodes
{
    public sealed class BehaviourNode : Node
    {
        public static string Category => NodeCategory.Scene;
        public BehaviourNode(NodeId id, NodeBehaviour nodeBehaviour) : base(id, nodeBehaviour.Name)
        {
            var inputs = nodeBehaviour.GetInputs();
            var outputs = nodeBehaviour.GetOutputs();

            InputSlots = new InputSlot[inputs.Length];
            for (var i = 0; i < inputs.Length; i++)
            {
                InputSlots[i] = SlotConverter.Convert(id, i, inputs[i], IsMuted);
            }

            OutputSlots = new OutputSlot[outputs.Length];
            for (var i = 0; i < outputs.Length; i++)
            {
                OutputSlots[i] = SlotConverter.Convert(id, i, outputs[i], IsMuted);
            }
        }

        public override InputSlot[] InputSlots { get; }
        public override OutputSlot[] OutputSlots { get; }
    }
}
