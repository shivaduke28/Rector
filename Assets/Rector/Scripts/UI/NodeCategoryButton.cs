using System;
using R3;
using UnityEngine;
using UnityEngine.UIElements;

namespace Rector.UI
{
    [UxmlElement("NodeCategoryButton")]
    public partial class NodeCategoryButton : VisualElement
    {
        readonly Button button = new();
        readonly VisualElement icon = new();
        readonly Label label = new();
        
        const string UssClassName = "rector-node-category-button";
        const string UssClassNameFocused = "rector-node-category-button--focused";
        const string UssClassNameHighlighted = "rector-node-category-button--highlighted";
        const string IconClassName = "rector-node-category-button-icon";
        const string ButtonClassName = "rector-node-category-button-inner";

        public NodeCategoryButton()
        {
            AddToClassList(UssClassName);
            AddToClassList(UssClassNameFocused);
            AddToClassList(UssClassNameHighlighted);

            EnableInClassList(UssClassNameFocused, false);
            EnableInClassList(UssClassNameHighlighted, false);

            button.AddToClassList(ButtonClassName);
            icon.AddToClassList(IconClassName);
            label.AddToClassList("rector-label");

            button.Add(icon);
            button.Add(label);
            Add(button);
        }

        public IDisposable Bind(NodeCategoryButtonState state)
        {
            return new CompositeDisposable(
                Observable.FromEvent(h => button.clicked += h, h => button.clicked -= h).Subscribe(_ => state.OnClick()),
                state.IsFocused.Subscribe(x => EnableInClassList(UssClassNameFocused, x)),
                state.IsHighlighted.Subscribe(x => EnableInClassList(UssClassNameHighlighted, x)),
                state.Text.Subscribe(x => label.text = x),
                state.Icon.Subscribe(x =>
                {
                    if (x != null)
                    {
                        icon.style.backgroundImage = new StyleBackground(x);
                        icon.style.display = DisplayStyle.Flex;
                    }
                    else
                    {
                        icon.style.display = DisplayStyle.None;
                    }
                }));
        }
    }
}