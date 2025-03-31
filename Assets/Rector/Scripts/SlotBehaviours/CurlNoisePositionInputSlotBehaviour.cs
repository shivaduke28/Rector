using Rector.NodeBehaviours;
using UnityEngine;

namespace Rector.SlotBehaviours
{
    [AddComponentMenu("Rector/Curl Noise Position Input Slot")]
    public sealed class CurlNoisePositionInputSlotBehaviour : InputSlotBehaviour
    {
        [SerializeField] FloatInput radius = new("Radius", 5f, 0f, 10f);
        [SerializeField] Vector3Input offset = new("Offset", new Vector3(0, 3, 0));
        [SerializeField] FloatInput speed = new("Speed", 0.2f, 0f, 1f);
        Vector3 target;

        Transform trans;

        IInput[] inputs;

        void Start()
        {
            trans = transform;
        }

        public void Update()
        {
            var offsetValue = offset.Value.Value;
            var radiusValue = radius.Value.Value;

            var t = Time.realtimeSinceStartup * speed.Value.Value;
            var lp = Noise.CurlNoise(t, t, t) * radiusValue + offsetValue;
            var diff = lp - trans.localPosition;
            if (diff.sqrMagnitude > 0.001f)
            {
                trans.localRotation = Quaternion.LookRotation(diff);
            }

            trans.localPosition = Noise.CurlNoise(t, t, t) * radiusValue + offsetValue;
        }

        public override IInput[] GetInputs()
        {
            return inputs ??= new IInput[]
            {
                radius,
                offset,
                speed,
            };
        }
    }
}
