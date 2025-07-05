using Rector.UI.Graphs.Slots;
using Rector.Vfx;

namespace Rector.UI.Graphs.Nodes
{
    public sealed class VfxNode : Node
    {
        public static NodeCategory GetCategory() => NodeCategory.Vfx;
        public override NodeCategory Category => GetCategory();
        readonly VfxNodeBehaviour behaviour;

        public VfxNode(NodeId id, VfxNodeBehaviour behaviour) : base(id, behaviour.Name)
        {
            this.behaviour = behaviour;
            var inputs = behaviour.GetInputs();
            var outputs = behaviour.GetOutputs();

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

        public override void DoAction()
        {
            behaviour.ToggleActive();
        }
    }
}
