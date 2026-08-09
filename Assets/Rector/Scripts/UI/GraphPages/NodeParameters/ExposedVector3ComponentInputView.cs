using System;
using R3;
using UnityEngine.UIElements;

namespace Rector.UI.GraphPages.NodeParameters
{
    public sealed class ExposedVector3ComponentInputView
    {
        readonly VisualElement root;
        readonly Label nameLabel;
        readonly RectorSlider slider;
        readonly Label valueLabel;
        readonly Label stepLabel;

        RectorSliderState sliderState;

        public ExposedVector3ComponentInputView(VisualElement container)
        {
            root = container.Q<VisualElement>("input");
            nameLabel = root.Q<Label>("name-label");
            slider = root.Q<RectorSlider>("slider");
            valueLabel = root.Q<Label>("value-label");
            stepLabel = root.Q<Label>("step-label");
        }

        public IDisposable Bind(ExposedVector3ComponentInputModel model)
        {
            nameLabel.text = model.Label;
            // 読みは成分の写し、書きはスロットへ返す。往復しないので同期のガードは要らない。
            sliderState = new RectorSliderState(model.Value, model.Write, model.MinValue, model.MaxValue);
            var format = model.ValueFormat;
            return new CompositeDisposable(
                slider.Bind(sliderState),
                model.Value.Subscribe(x => valueLabel.text = x.ToString(format)),
                model.IsFocused.Subscribe(x => root.EnableInClassList("rector-exposed-input--focused", x)),
                model.StepType.Subscribe(x => stepLabel.text = ExposedStepCalculator.StepLabel(model.StepSize(x)))
            );
        }

        public void AddTo(VisualElement parent) => parent.Add(root);
    }
}
