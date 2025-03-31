using System;
using Rector.NodeBehaviours;
using Rector.UI.Graphs.Slots;

namespace Rector.UI.Graphs.Nodes
{
    public sealed class BehaviourNode : Node
    {
        public BehaviourNode(NodeId id, NodeBehaviour nodeBehaviour) : base(id, nodeBehaviour == null ? "Destroyed" : nodeBehaviour.Name)
        {
            if (nodeBehaviour == null)
            {
                InputSlots = Array.Empty<InputSlot>();
                OutputSlots = Array.Empty<OutputSlot>();
                return;
            }

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
