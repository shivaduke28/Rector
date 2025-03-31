using R3;
using Rector.NodeBehaviours;
using Unity.Cinemachine;
using UnityEngine;

namespace Rector.Cameras
{
    [RequireComponent(typeof(CinemachineCamera))]
    public sealed class CameraTrackingTargetInputBehaviour : InputBehaviour
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
