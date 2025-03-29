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
        public readonly ReadOnlyReactiveProperty<SliderStepType> StepType;
        public readonly ReactiveProperty<bool> IsFocused = new(false);

        readonly int digit;

        public ExposedFloatInputModel(ReactivePropertyFloatInputSlot slot, ReadOnlyReactiveProperty<SliderStepType> stepType)
        {
            Slot = slot;
            StepType = stepType;
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

        public void Focus() => IsFocused.Value = true;

        public void Unfocus() => IsFocused.Value = false;
    }
}
