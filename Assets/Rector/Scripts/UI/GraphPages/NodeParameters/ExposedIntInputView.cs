using System;
using R3;
using UnityEngine.UIElements;

namespace Rector.UI.GraphPages.NodeParameters
{
    public sealed class ExposedIntInputView
    {
        readonly VisualElement root;
        readonly Label nameLabel;
        readonly RectorSliderInt slider;
        readonly Label valueLabel;

        RectorSliderIntState sliderState;

        public ExposedIntInputView(VisualElement container)
        {
            root = container.Q<VisualElement>("input");
            nameLabel = root.Q<Label>("name-label");
            slider = root.Q<RectorSliderInt>("slider");
            valueLabel = root.Q<Label>("value-label");
        }

        public IDisposable Bind(ExposedIntInputModel model)
        {
            var slot = model.Slot;
            nameLabel.text = slot.Name;
            sliderState = new RectorSliderIntState(slot.Property, slot.MinValue, slot.MaxValue);
            return new CompositeDisposable(
                slider.Bind(sliderState),
                slot.Property.Subscribe(x => valueLabel.text = x.ToString()),
                model.IsFocused.Subscribe(x => root.EnableInClassList("rector-exposed-input--focused", x))
            );
        }

        public void AddTo(VisualElement parent) => parent.Add(root);
    }
}