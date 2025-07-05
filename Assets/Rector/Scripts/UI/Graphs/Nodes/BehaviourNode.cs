using System;
using Rector.NodeBehaviours;
using Rector.UI.Graphs.Slots;

namespace Rector.UI.Graphs.Nodes
{
    public sealed class BehaviourNode : Node
    {
        readonly NodeBehaviourProxy proxy;

        public BehaviourNode(NodeId id, NodeBehaviourProxy proxy) : base(id, proxy.IsDestroyed ? "Destroyed" : proxy.Name)
        {
            this.proxy = proxy;

            var inputs = proxy.GetInputs();
            var outputs = proxy.GetOutputs();

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

        public override NodeCategory Category => proxy.Category;
        public override InputSlot[] InputSlots { get; }
        public override OutputSlot[] OutputSlots { get; }
    }
}
