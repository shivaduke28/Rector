using System;
using R3;
using Rector.Cameras;
using Rector.UI.Graphs.Slots;

namespace Rector.UI.Graphs.Nodes
{
    public sealed class CameraNode : Node
    {
        public static NodeCategory GetCategory() => NodeCategory.Camera;
        public override NodeCategory Category => GetCategory();
        public override InputSlot[] InputSlots { get; }
        public override OutputSlot[] OutputSlots => Array.Empty<OutputSlot>();

        readonly CameraNodeBehaviour cameraNodeBehaviour;
        IDisposable disposable;


        public CameraNode(NodeId id, CameraNodeBehaviour cameraNodeBehaviour) : base(id, cameraNodeBehaviour.Name)
        {
            this.cameraNodeBehaviour = cameraNodeBehaviour;
            var inputs = cameraNodeBehaviour.GetInputs();
            InputSlots = new InputSlot[inputs.Length];
            for (var i = 0; i < InputSlots.Length; i++)
            {
                var input = inputs[i];
                InputSlots[i] = SlotConverter.Convert(id, i, input, IsMuted);
            }
        }

        public override void DoAction()
        {
            cameraNodeBehaviour.IsActive.Value = true;
        }
    }
}
