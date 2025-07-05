using R3;
using Rector.NodeBehaviours;
using UnityEngine;

namespace Rector.SlotBehaviours
{
    [AddComponentMenu("Rector/Rotation Input Slot")]
    public sealed class RotationInputSlotBehaviour : InputSlotBehaviour
    {
        [SerializeField] Vector3Input input;

        Transform trans;

        void Start()
        {
            trans = transform;

            input.Value.Subscribe(r =>
            {
                trans.localRotation = Quaternion.Euler(r);
            }).AddTo(this);
        }

        public override IInput[] GetInputs()
        {
            return new IInput[]
            {
                input,
            };
        }

        void Reset()
        {
            input = new Vector3Input("Rotation", transform.localRotation.eulerAngles);
        }
    }
}
