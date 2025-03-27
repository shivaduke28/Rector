using R3;
using Rector.Nodes;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.Serialization;

namespace Rector.PostProcess
{
    [RequireComponent(typeof(Volume))]
    public sealed class ColorGradingInputBehaviour : InputBehaviour
    {
        [SerializeField] Volume volume;

        [FormerlySerializedAs("postExposure")] [SerializeField]
        FloatInput exposure;

        [SerializeField] FloatInput contrast;
        [SerializeField] FloatInput saturation;

        ColorAdjustments colorAdjustments;
        CallbackInput reset;

        IInput[] inputs;

        public override IInput[] GetInputs()
        {
            return inputs ??= new IInput[]
            {
                reset, exposure, contrast, saturation
            };
        }

        void Start()
        {
            reset = new CallbackInput("Reset", ResetParams);
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
