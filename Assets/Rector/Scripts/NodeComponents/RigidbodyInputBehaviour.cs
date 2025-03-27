using Rector.Nodes;
using UnityEngine;

namespace Rector.NodeComponents
{
    [RequireComponent(typeof(Rigidbody))]
    public sealed class RigidbodyInputBehaviour : InputBehaviour
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
