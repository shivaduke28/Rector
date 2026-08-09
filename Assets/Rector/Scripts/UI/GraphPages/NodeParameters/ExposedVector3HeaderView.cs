using System;
using R3;
using UnityEngine.UIElements;

namespace Rector.UI.GraphPages.NodeParameters
{
    public sealed class ExposedVector3HeaderView
    {
        readonly VisualElement root;
        readonly Label nameLabel;

        public ExposedVector3HeaderView(VisualElement container)
        {
            root = container.Q<VisualElement>("header");
            nameLabel = root.Q<Label>("name-label");
        }

        public IDisposable Bind(ExposedVector3HeaderRow row)
        {
            nameLabel.text = row.Label;
            return Disposable.Empty;
        }

        public void AddTo(VisualElement parent) => parent.Add(root);
    }
}
