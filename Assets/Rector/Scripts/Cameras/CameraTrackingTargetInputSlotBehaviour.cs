using R3;
using Rector.NodeBehaviours;
using Rector.SlotBehaviours;
using Unity.Cinemachine;
using UnityEngine;

namespace Rector.Cameras
{
    [AddComponentMenu("Rector/Camera Tracking Target Input Slot")]
    [RequireComponent(typeof(CinemachineCamera))]
    public sealed class CameraTrackingTargetInputSlotBehaviour : InputSlotBehaviour
    {
        [SerializeField] CinemachineCamera cinemachineCamera;
        [SerializeField] TransformInput trackingTarget;

        IInput[] inputs;

        public override IInput[] GetInputs()
        {
            return inputs ??= new IInput[]
            {
                trackingTarget
            };
        }

        void Start()
        {
            trackingTarget.Value
                .Subscribe(t => cinemachineCamera.Target = new CameraTarget { TrackingTarget = t })
                .AddTo(this);
        }


        void Reset()
        {
            cinemachineCamera = GetComponent<CinemachineCamera>();
            trackingTarget = new TransformInput("Target", cinemachineCamera.Target.TrackingTarget);
            cinemachineCamera.Priority = new PrioritySettings { Enabled = true, Value = 0 };
            cinemachineCamera.StandbyUpdate = CinemachineVirtualCameraBase.StandbyUpdateMode.Always;
        }
    }
}
