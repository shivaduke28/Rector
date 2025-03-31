using R3;
using Rector.Cameras;
using Rector.NodeBehaviours;
using UnityEngine;

namespace Rector.NodeComponents
{
    [RequireComponent(typeof(CinemachineRandomSphereTarget))]
    public sealed class CinemachineRandomSphereTargetInputBehaviour : InputBehaviour
    {
        [SerializeField] CinemachineRandomSphereTarget cinemachineRandomSphereTarget;
        [SerializeField] FloatInput speed = new("Speed", 3f, 0f, 10f);
        [SerializeField] FloatInput radius = new("Radius", 1f, 0f, 10f);
        [SerializeField] FloatInput period = new("Period", 1f, 0f, 4f);

        IInput[] inputs;
        public override IInput[] GetInputs()
        {
            return inputs ??= new IInput[]
            {
                speed,
                radius,
                period,
            };
        }

        void Reset()
        {
            cinemachineRandomSphereTarget = GetComponent<CinemachineRandomSphereTarget>();
        }

        void Start()
        {
            speed.Value.Subscribe(x => cinemachineRandomSphereTarget.speed = x).AddTo(this);
            radius.Value.Subscribe(x => cinemachineRandomSphereTarget.radius = x).AddTo(this);
            period.Value.Subscribe(x => cinemachineRandomSphereTarget.period = x).AddTo(this);
        }
    }
}
