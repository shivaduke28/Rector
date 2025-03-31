using R3;
using Rector.NodeBehaviours;
using UnityEngine;

namespace Rector.SlotBehaviours
{
    [AddComponentMenu("Rector/Light Input Slot")]
    [RequireComponent(typeof(Light))]
    public sealed class LightInputSlotBehaviour : InputSlotBehaviour
    {
        [SerializeField] new Light light;
        [SerializeField] FloatInput intensity;
        [SerializeField] FloatInput range;

        IInput[] inputs;

        void Start()
        {
            intensity.Value.Subscribe(x => light.intensity = x).AddTo(this);
            range.Value.Subscribe(x => light.range = x).AddTo(this);
        }

        public override IInput[] GetInputs()
        {
            return inputs ??= new IInput[]
            {
                new CallbackInput("Reset", ResetParams),
                intensity,
                range,
            };
        }

        void ResetParams()
        {
            intensity.Value.Value = intensity.DefaultValue;
            range.Value.Value = range.DefaultValue;
        }

        void Reset()
        {
            light = GetComponent<Light>();
            intensity = new FloatInput("Intensity", light.intensity, 0f, 30f);
            range = new FloatInput("Range", light.range, 0f, 100f);
        }
    }
}
