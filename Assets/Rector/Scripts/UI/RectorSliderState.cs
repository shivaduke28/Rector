using R3;

namespace Rector.UI
{
    public sealed class RectorSliderState
    {
        public readonly ReactiveProperty<bool> IsFocused = new(false);
        public readonly ReactiveProperty<bool> IsHighlighted = new(false);
        public readonly ReactiveProperty<float> Value;

        public readonly float MinValue;
        public readonly float MaxValue;

        public RectorSliderState(
            ReactiveProperty<float> property,
            float minValue,
            float maxValue)
        {
            Value = property;
            MinValue = minValue;
            MaxValue = maxValue;
        }
    }
    
    
    public sealed class RectorSliderIntState
    {
        public readonly ReactiveProperty<bool> IsFocused = new(false);
        public readonly ReactiveProperty<bool> IsHighlighted = new(false);
        public readonly ReactiveProperty<int> Value;

        public readonly int MinValue;
        public readonly int MaxValue;

        public RectorSliderIntState(
            ReactiveProperty<int> property,
            int minValue,
            int maxValue)
        {
            Value = property;
            MinValue = minValue;
            MaxValue = maxValue;
        }
    }
}
