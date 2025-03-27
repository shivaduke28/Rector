using Unity.Cinemachine;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Rector.Cameras
{
    [DisallowMultipleComponent]
    [CameraPipeline(CinemachineCore.Stage.Body)]
    [RequiredTarget(RequiredTargetAttribute.RequiredTargets.LookAt)]
    public sealed class CinemachineRandomSphereTarget : CinemachineComponentBase
    {
        public float speed = 0.2f;
        public float radius = 1f;
        [Range(0, 4f)] public float period = 1f;
        float t;
        Vector3 to;

        public override void MutateCameraState(ref CameraState curState, float deltaTime)
        {
            if (IsValid && curState.HasLookAt())
            {
                t += Time.deltaTime;
                if (t > period)
                {
                    t -= period;
                    to = Random.insideUnitSphere * radius;
                }

                curState.RawPosition = Vector3.Lerp(curState.GetCorrectedPosition(), LookAtTarget.position + to,
                    Time.deltaTime * speed);
            }
        }

        public override bool IsValid => LookAtTarget != null;
        public override CinemachineCore.Stage Stage => CinemachineCore.Stage.Body;
    }
}
