using System;
using R3;
using UnityEngine.UIElements;

namespace Rector.UI
{
    [UxmlElement("RectorSlider")]
    public sealed partial class RectorSlider : VisualElement
    {
        readonly Slider slider = new();
        const string UssClassName = "rector-slider";
        const string UssClassNameFocused = "rector-slider--focused";
        const string UssClassNameHighlighted = "rector-slider--highlighted";

        public RectorSlider()
        {
            AddToClassList(UssClassName);
            AddToClassList(UssClassNameFocused);
            AddToClassList(UssClassNameHighlighted);

            EnableInClassList(UssClassNameFocused, false);
            EnableInClassList(UssClassNameHighlighted, false);

            Add(slider);
        }

        public IDisposable Bind(RectorSliderState state)
        {
            if (float.IsFinite(state.MinValue) && float.IsFinite(state.MaxValue))
            {
                slider.lowValue = state.MinValue;
                slider.highValue = state.MaxValue;
            }
            else
            {
                slider.visible = false;
            }
            return new CompositeDisposable(
                state.Value.Subscribe(x => slider.SetValueWithoutNotify(x)),
                slider.OnValueChangeAsObservable().Subscribe(x => state.Value.Value = x.newValue),
                state.IsFocused.Subscribe(x => EnableInClassList(UssClassNameFocused, x)),
                state.IsHighlighted.Subscribe(x => EnableInClassList(UssClassNameHighlighted, x))
            );
        }
    }

    [UxmlElement("RectorSliderInt")]
    public sealed partial class RectorSliderInt : VisualElement
    {
        readonly SliderInt slider = new();
        const string UssClassName = "rector-slider";
        const string UssClassNameFocused = "rector-slider--focused";
        const string UssClassNameHighlighted = "rector-slider--highlighted";

        public RectorSliderInt()
        {
            AddToClassList(UssClassName);
            AddToClassList(UssClassNameFocused);
            AddToClassList(UssClassNameHighlighted);

            EnableInClassList(UssClassNameFocused, false);
            EnableInClassList(UssClassNameHighlighted, false);

            Add(slider);
        }

        public IDisposable Bind(RectorSliderIntState state)
        {
            slider.lowValue = state.MinValue;
            slider.highValue = state.MaxValue;
            return new CompositeDisposable(
                state.Value.Subscribe(x => slider.SetValueWithoutNotify(x)),
                slider.OnValueChangeAsObservable().Subscribe(x => state.Value.Value = x.newValue),
                state.IsFocused.Subscribe(x => EnableInClassList(UssClassNameFocused, x)),
                state.IsHighlighted.Subscribe(x => EnableInClassList(UssClassNameHighlighted, x))
            );
        }
    }
}
