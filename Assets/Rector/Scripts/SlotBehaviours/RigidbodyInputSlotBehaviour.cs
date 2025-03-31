using Rector.NodeBehaviours;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Rector.SlotBehaviours
{
    [AddComponentMenu("Rector/Rigitbody Input Slot")]
    [RequireComponent(typeof(Rigidbody))]
    public sealed class RigidbodyInputSlotBehaviour : InputSlotBehaviour
    {
        [SerializeField] Rigidbody rb;
        [SerializeField] FloatInput power = new("Power", 1f, 0f, 10f);
        IInput[] inputs;

        void AddForce()
        {
            if (rb != null)
            {
                var dir = Random.onUnitSphere;
                dir.y = Mathf.Abs(dir.y);
                rb.AddForce(dir * power.Value.Value, ForceMode.Impulse);
            }
        }

        void Reset() => rb = GetComponent<Rigidbody>();

        public override IInput[] GetInputs()
        {
            return inputs ??= new IInput[]
            {
                new CallbackInput("Add Force", AddForce),
                power,
            };
        }
    }
}
