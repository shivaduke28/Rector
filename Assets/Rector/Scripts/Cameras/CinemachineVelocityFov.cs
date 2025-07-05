using Unity.Cinemachine;
using UnityEngine;

namespace Rector.Cameras
{
    public sealed class CinemachineVelocityFov : CinemachineExtension
    {
        [SerializeField] float minFov = 60f;
        [SerializeField] float maxFov = 120f;
        [SerializeField] float minVelocity = 0f;
        [SerializeField] float maxVelocity = 10f;
        [SerializeField] AnimationCurve curve = AnimationCurve.Linear(0, 0, 1, 1);

        Vector3 lastPosition;

        // control fov based on velocity
        protected override void PostPipelineStageCallback(CinemachineVirtualCameraBase vcam, CinemachineCore.Stage stage, ref CameraState state,
            float deltaTime)
        {
            if (stage == CinemachineCore.Stage.Finalize)
            {
                var velocity = (state.RawPosition - lastPosition) / deltaTime;
                lastPosition = state.RawPosition;

                var t = Mathf.InverseLerp(minVelocity, maxVelocity, velocity.magnitude);
                var lens = state.Lens;
                lens.FieldOfView = Mathf.Lerp(minFov, maxFov, curve.Evaluate(t));
                state.Lens = lens;
            }
        }
    }
}
