using R3;
using Rector.NodeBehaviours;
using UnityEngine;

namespace Rector.SlotBehaviours
{
    [AddComponentMenu("Rector/Position Input Slot")]
    public sealed class PositionInputSlotBehaviour : InputSlotBehaviour
    {
        [SerializeField] Vector3Input input;
        [SerializeField] bool reinit;

        Transform trans;

        void Start()
        {
            trans = transform;

            input.Value.Subscribe(p =>
            {
                trans.localPosition = p;
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
            input = new Vector3Input("Position", transform.localPosition);
        }
    }
}
