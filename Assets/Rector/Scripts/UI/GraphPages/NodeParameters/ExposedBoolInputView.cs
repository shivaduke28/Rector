using System;
using R3;
using UnityEngine.UIElements;

namespace Rector.UI.GraphPages.NodeParameters
{
    public sealed class ExposedBoolInputView
    {
        readonly VisualElement root;
        readonly Label nameLabel;
        readonly Label valueLabel;
        readonly RectorToggle toggle;

        public ExposedBoolInputView(VisualElement container)
        {
            root = container.Q<VisualElement>("input");
            nameLabel = root.Q<Label>("name-label");
            toggle = root.Q<RectorToggle>("toggle");
            valueLabel = root.Q<Label>("value-label");
        }

        public IDisposable Bind(ExposedBoolInputModel model)
        {
            var slot = model.Slot;
            nameLabel.text = slot.Name;
            return new CompositeDisposable(
                toggle.Bind(model.ToggleState),
                model.IsFocused.Subscribe(x => root.EnableInClassList("rector-exposed-input--focused", x)),
                model.ToggleState.Value.Subscribe(x => valueLabel.text = x ? "true" : "false")
            );
        }

        public void AddTo(VisualElement parent) => parent.Add(root);
    }
}
