using System;
using R3;
using Rector.UI.Graphs.Nodes;
using Rector.UI.Graphs.Slots;
using UnityEngine;

namespace Rector.UI.GraphPages.NodeParameters
{
    public sealed class ExposedFloatInputModel : IExposedInputModel
    {
        public readonly ReactivePropertyFloatInputSlot Slot;
        public ReadOnlyReactiveProperty<SliderStepType> StepType => stepType;
        public readonly ReactiveProperty<bool> IsFocused = new(false);

        readonly ReactiveProperty<SliderStepType> stepType;
        readonly int digit;

        public ExposedFloatInputModel(ReactivePropertyFloatInputSlot slot, ReactiveProperty<SliderStepType> stepType)
        {
            Slot = slot;
            this.stepType = stepType;
            var diff = slot.MaxValue - slot.MinValue;

            digit = diff switch
            {
                >= 10f => 0, // 整数
                >= 1f => 1, // 小数第一位
                _ => 2 // 小数第二位
            };
        }

        int StepToDigit(SliderStepType step)
        {
            return step switch
            {
                SliderStepType.Times1 => digit,
                SliderStepType.Times10 => digit + 1,
                SliderStepType.Times100 => digit + 2,
                _ => digit
            };
        }

        public void Increment()
        {
            var d = StepToDigit(StepType.CurrentValue);
            var result = Math.Round(Slot.Property.Value, d);
            result += Math.Pow(10, -d);

            Slot.Property.Value = Mathf.Clamp((float)result, Slot.MinValue, Slot.MaxValue);
        }

        public void Decrement()
        {
            var d = StepToDigit(StepType.CurrentValue);
            var rounded = Math.Round(Slot.Property.Value, d);
            rounded -= Math.Pow(10, -d);

            Slot.Property.Value = Mathf.Clamp((float)rounded, Slot.MinValue, Slot.MaxValue);
        }

        /// <summary>刻み幅を x1 -> x10 -> x100 と回す。</summary>
        public void DoAction()
        {
            stepType.Value = stepType.CurrentValue switch
            {
                SliderStepType.Times1 => SliderStepType.Times10,
                SliderStepType.Times10 => SliderStepType.Times100,
                _ => SliderStepType.Times1
            };
        }

        public void Focus() => IsFocused.Value = true;

        public void Unfocus() => IsFocused.Value = false;
    }
}
