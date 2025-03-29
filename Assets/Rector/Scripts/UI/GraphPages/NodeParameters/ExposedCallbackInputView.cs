using System;
using R3;
using UnityEngine.UIElements;

namespace Rector.UI.GraphPages.NodeParameters
{
    public sealed class ExposedCallbackInputView
    {
        readonly VisualElement root;
        readonly Label nameLabel;

        public ExposedCallbackInputView(VisualElement container)
        {
            root = container.Q<VisualElement>("input");
            nameLabel = root.Q<Label>("name-label");
        }

        public IDisposable Bind(ExposedCallbackInputModel model)
        {
            var slot = model.Slot;
            nameLabel.text = slot.Name;
            return new CompositeDisposable(
                model.IsFocused.Subscribe(x => root.EnableInClassList("rector-exposed-input--focused", x))
            );
        }

        public void AddTo(VisualElement parent) => parent.Add(root);
    }
}
