using System;
using R3;
using UnityEngine.UIElements;

namespace Rector.UI.GraphPages.NodeParameters
{
    public sealed class ExposedFloatInputView
    {
        readonly VisualElement root;
        readonly Label nameLabel;
        readonly RectorSlider slider;
        readonly Label valueLabel;
        readonly Label stepLabel;

        RectorSliderState sliderState;

        public ExposedFloatInputView(VisualElement container)
        {
            root = container.Q<VisualElement>("input");
            nameLabel = root.Q<Label>("name-label");
            slider = root.Q<RectorSlider>("slider");
            valueLabel = root.Q<Label>("value-label");
            stepLabel = root.Q<Label>("step-label");
        }

        public IDisposable Bind(ExposedFloatInputModel model)
        {
            var slot = model.Slot;
            nameLabel.text = slot.Name;
            sliderState = new RectorSliderState(slot.Property, slot.MinValue, slot.MaxValue);
            var diff = slot.MaxValue - slot.MinValue;
            var format = diff switch
            {
                >= 10f => "F2",
                >= 1f => "F3",
                _ => "F4"
            };
            return new CompositeDisposable(
                slider.Bind(sliderState),
                slot.Property.Subscribe(x => valueLabel.text = x.ToString(format)),
                model.IsFocused.Subscribe(x => root.EnableInClassList("rector-exposed-input--focused", x)),
                model.StepType.Subscribe(x => stepLabel.text = $"±{Format(x)}")
            );
        }

        static string Format(SliderStepType step)
        {
            return step switch
            {
                SliderStepType.Times1 => "1",
                SliderStepType.Times10 => "0.1",
                SliderStepType.Times100 => "0.01",
                _ => throw new ArgumentOutOfRangeException(nameof(step), step, null)
            };
        }

        public void AddTo(VisualElement parent)
        {
            parent.Add(root);
        }
    }
}
