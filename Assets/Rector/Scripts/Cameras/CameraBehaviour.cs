using System.Linq;
using R3;
using Rector.NodeBehaviours;
using Unity.Cinemachine;
using UnityEngine;

namespace Rector.Cameras
{
    [RequireComponent(typeof(CinemachineCamera))]
    public sealed class CameraBehaviour : MonoBehaviour
    {
        [SerializeField] CinemachineCamera cinemachineCamera;
        [SerializeField] InputBehaviour[] inputBehaviours;

        [SerializeField] BoolInput activeInput = new("Active", false);
        [SerializeField] FloatInput dutchInput = new("Dutch", 0, -180, 180);

        public ReactiveProperty<bool> IsActive => activeInput.Value;
        public string Name => name;

        public IInput[] GetInputs()
        {
            return inputBehaviours.SelectMany(x => x.GetInputs())
                .Prepend(dutchInput)
                .Prepend(activeInput).ToArray();
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
            inputBehaviours = GetComponentsInChildren<InputBehaviour>();
            dutchInput.Value.Value = cinemachineCamera.Lens.Dutch;
        }
    }
}
