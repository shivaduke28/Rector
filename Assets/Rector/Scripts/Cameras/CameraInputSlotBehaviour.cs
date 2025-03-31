using R3;
using Rector.NodeBehaviours;
using Rector.SlotBehaviours;
using Unity.Cinemachine;
using UnityEngine;

namespace Rector.Cameras
{
    [AddComponentMenu("Rector/Camera Input Slot")]
    [RequireComponent(typeof(CinemachineCamera))]
    public sealed class CameraInputSlotBehaviour : InputSlotBehaviour
    {
        [SerializeField] CinemachineCamera cinemachineCamera;

        [SerializeField] BoolInput activeInput = new("Active", false);
        [SerializeField] FloatInput dutchInput = new("Dutch", 0, -180, 180);

        IInput[] inputs;
        public BoolInput ActiveInput => activeInput;

        public override IInput[] GetInputs()
        {
            return inputs ??= new IInput[]
            {
                dutchInput,
                activeInput
            };
        }

        void Start()
        {
            activeInput.Value.Subscribe(x => cinemachineCamera.Priority = x ? 1 : 0).AddTo(this);
            dutchInput.Value.Subscribe(UpdateDutch).AddTo(this);
        }

        void UpdateDutch(float dutch)
        {
            // wrap -180 to 180
            while (dutch < -180)
            {
                dutch += 360;
            }

            while (dutch > 180)
            {
                dutch -= 360;
            }

            var lens = cinemachineCamera.Lens;
            lens.Dutch = dutch;
            cinemachineCamera.Lens = lens;
        }

        void Reset()
        {
            cinemachineCamera = GetComponent<CinemachineCamera>();
            dutchInput.Value.Value = cinemachineCamera.Lens.Dutch;
        }
    }
}
