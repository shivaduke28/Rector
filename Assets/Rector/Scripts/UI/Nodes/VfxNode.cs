using System;
using R3;
using Rector.Nodes;
using Rector.UI.Graphs;

namespace Rector.UI.Nodes
{
    public sealed class VfxNode : Node, IInitializable, IDisposable
    {
        readonly VfxNodeBehaviour behaviour;
        readonly BoolInput activeInput = new("Active", false);
        IDisposable disposable;

        public VfxNode(NodeId id, VfxNodeBehaviour behaviour) : base(id, behaviour.Name)
        {
            this.behaviour = behaviour;
            var inputs = behaviour.GetInputs();
            var outputs = behaviour.GetOutputs();

            InputSlots = new InputSlot[inputs.Length + 1];
            InputSlots[0] = SlotConverter.Convert(id, 0, activeInput, IsMuted);
            for (var i = 0; i < inputs.Length; i++)
            {
                InputSlots[i + 1] = SlotConverter.Convert(id, i + 1, inputs[i], IsMuted);
            }

            OutputSlots = new OutputSlot[outputs.Length];
            for (var i = 0; i < outputs.Length; i++)
            {
                OutputSlots[i] = SlotConverter.Convert(id, i, outputs[i], IsMuted);
            }
        }

        public override InputSlot[] InputSlots { get; }
        public override OutputSlot[] OutputSlots { get; }

        public void Initialize()
        {
            disposable = activeInput.Value
                .Subscribe(x => behaviour.SetActive(x));
        }

        public override void DoAction()
        {
            activeInput.Value.Value = !activeInput.Value.Value;
        }

        public void Dispose() => disposable?.Dispose();
    }
}
