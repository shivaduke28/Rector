using R3;
using Rector.UI.Graphs.Slots;

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
            digit = ExposedStepCalculator.DigitFromRange(slot.MinValue, slot.MaxValue);
        }

        public string Label => Slot.Name;

        public string ValueFormat => ExposedStepCalculator.ValueFormat(digit);

        public float StepSize(SliderStepType step) => ExposedStepCalculator.StepSize(digit, step);

        public void Increment() => Move(true);

        public void Decrement() => Move(false);

        void Move(bool increment)
        {
            Slot.Property.Value = ExposedStepCalculator.Apply(
                Slot.Property.Value, digit, stepType.CurrentValue, increment, Slot.MinValue, Slot.MaxValue);
        }

        /// <summary>刻み幅を x1 -> x10 -> x100 と回す。</summary>
        public void DoAction() => stepType.Value = ExposedStepCalculator.Next(stepType.CurrentValue);

        public void Focus() => IsFocused.Value = true;

        public void Unfocus() => IsFocused.Value = false;
    }
}
