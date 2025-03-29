using System;
using R3;
using Rector.Cameras;

namespace Rector.UI.Graphs.Nodes
{
    public sealed class CameraNode : Node
    {
        public static string Category => NodeCategoryV2.Camera;
        public override InputSlot[] InputSlots { get; }
        public override OutputSlot[] OutputSlots => Array.Empty<OutputSlot>();

        readonly CameraBehaviour cameraBehaviour;
        IDisposable disposable;


        public CameraNode(NodeId id, CameraBehaviour cameraBehaviour) : base(id, cameraBehaviour.Name)
        {
            this.cameraBehaviour = cameraBehaviour;
            var inputs = cameraBehaviour.GetInputs();
            InputSlots = new InputSlot[inputs.Length];
            for (var i = 0; i < InputSlots.Length; i++)
            {
                var input = inputs[i];
                InputSlots[i] = SlotConverter.Convert(id, i, input, IsMuted);
            }
        }

        public override void DoAction()
        {
            cameraBehaviour.IsActive.Value = true;
        }
    }
}
