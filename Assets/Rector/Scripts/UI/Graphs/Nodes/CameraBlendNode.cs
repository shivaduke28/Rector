using System;
using Rector.Cameras;
using Rector.UI.Graphs.Slots;

namespace Rector.UI.Graphs.Nodes
{
    public sealed class CameraBlendNode : Node
    {
        public const string NodeName = "Camera Blend";
        public static string Category => NodeCategory.Camera;
        public override InputSlot[] InputSlots { get; }
        public override OutputSlot[] OutputSlots => Array.Empty<OutputSlot>();

        public CameraBlendNode(NodeId id, CameraManager cameraManager) : base(id, NodeName)
        {
            var blendInputs = cameraManager.BlendInputs;
            InputSlots = new InputSlot[blendInputs.Length + 1];
            for (var i = 0; i < blendInputs.Length; i++)
            {
                var blendInput = blendInputs[i];
                InputSlots[i] = SlotConverter.Convert(id, i, blendInput, IsMuted);
            }

            InputSlots[^1] = new ReactivePropertyFloatInputSlot(id, blendInputs.Length, "Time", cameraManager.BlendTime, 1f, 0f, 10f, IsMuted);
        }
    }
}
