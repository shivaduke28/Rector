using System;
using R3;
using UnityEngine.UIElements;

namespace Rector.UI.GraphPages.NodeParameters
{
    public sealed class ExposedCallbackFloatInputView
    {
        readonly VisualElement root;
        readonly Label nameLabel;
        readonly RectorSlider slider;
        readonly Label valueLabel;
        readonly Label stepLabel;

        RectorSliderState sliderState;
        // スロットのLatestValueは読み取り専用なので、スライダー表示用にミラーする
        readonly ReactiveProperty<float> displayValue = new(0f);

        public ExposedCallbackFloatInputView(VisualElement container)
        {
            root = container.Q<VisualElement>("input");
            nameLabel = root.Q<Label>("name-label");
            slider = root.Q<RectorSlider>("slider");
            valueLabel = root.Q<Label>("value-label");
            stepLabel = root.Q<Label>("step-label");
        }

        public IDisposable Bind(ExposedCallbackFloatInputModel model)
        {
            var slot = model.Slot;
            nameLabel.text = model.Label;
            sliderState = new RectorSliderState(displayValue, slot.MinValue, slot.MaxValue);
            var format = model.ValueFormat;
            return new CompositeDisposable(
                slider.Bind(sliderState),
                slot.LatestValue.Subscribe(x =>
                {
                    displayValue.Value = x;
                    valueLabel.text = x.ToString(format);
                }),
                model.IsFocused.Subscribe(x => root.EnableInClassList("rector-exposed-input--focused", x)),
                model.StepType.Subscribe(x => stepLabel.text = ExposedStepCalculator.StepLabel(model.StepSize(x)))
            );
        }

        public void AddTo(VisualElement parent)
        {
            parent.Add(root);
        }
    }
}
