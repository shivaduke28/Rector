using R3;
using Rector.NodeBehaviours;
using Rector.SlotBehaviours;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace Rector.PostProcess
{
    [AddComponentMenu("Rector/URP Color Grading Input Slot")]
    [RequireComponent(typeof(Volume))]
    public sealed class UrpColorGradingInputSlotBehaviour : InputSlotBehaviour
    {
        [SerializeField] Volume volume;
        [SerializeField] FloatInput exposure;
        [SerializeField] FloatInput contrast;
        [SerializeField] FloatInput saturation;

        ColorAdjustments colorAdjustments;

        IInput[] inputs;

        public override IInput[] GetInputs()
        {
            if (inputs == null)
            {
                inputs = new IInput[]
                {
                    new CallbackInput("Reset", ResetParams), exposure, contrast, saturation
                };
            }
            return inputs;
        }

        void Start()
        {
            if (!volume.profile.TryGet(out colorAdjustments)) return;

            colorAdjustments.postExposure.overrideState = true;
            colorAdjustments.contrast.overrideState = true;
            colorAdjustments.saturation.overrideState = true;

            exposure.Value.Subscribe(x => colorAdjustments.postExposure.value = x).AddTo(this);
            contrast.Value.Subscribe(x => colorAdjustments.contrast.value = x).AddTo(this);
            saturation.Value.Subscribe(x => colorAdjustments.saturation.value = x).AddTo(this);
        }

        void ResetParams()
        {
            exposure.Value.Value = exposure.DefaultValue;
            contrast.Value.Value = contrast.DefaultValue;
            saturation.Value.Value = saturation.DefaultValue;
        }

        void Reset()
        {
            volume = GetComponent<Volume>();
            if (volume.profile.TryGet(out colorAdjustments))
            {
                exposure = new FloatInput("Exposure", colorAdjustments.postExposure.value, -8f, 8f);
                contrast = new FloatInput("Contrast", colorAdjustments.contrast.value, -100f, 100f);
                saturation = new FloatInput("Saturation", colorAdjustments.saturation.value, -100f, 100f);
            }
        }
    }
}
