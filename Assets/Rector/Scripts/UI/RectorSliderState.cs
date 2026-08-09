using System;
using R3;

namespace Rector.UI
{
    public sealed class RectorSliderState
    {
        public readonly ReactiveProperty<bool> IsFocused = new(false);
        public readonly ReactiveProperty<bool> IsHighlighted = new(false);

        /// <summary>スライダーが映す値。書き戻しは <see cref="Write"/> を通す。</summary>
        public readonly ReadOnlyReactiveProperty<float> Value;

        public readonly float MinValue;
        public readonly float MaxValue;

        readonly Action<float> write;

        public RectorSliderState(
            ReactiveProperty<float> property,
            float minValue,
            float maxValue) : this(property, value => property.Value = value, minValue, maxValue)
        {
        }

        /// <summary>
        /// 値の置き場とスライダーの持ち場が一致しないとき用。
        /// Vector3の1成分のように、読みは射影・書きは元の値の作り直しになる場合に使う。
        /// </summary>
        public RectorSliderState(
            ReadOnlyReactiveProperty<float> value,
            Action<float> write,
            float minValue,
            float maxValue)
        {
            Value = value;
            this.write = write;
            MinValue = minValue;
            MaxValue = maxValue;
        }

        public void Write(float value) => write(value);
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
