using System;
using R3;
using UnityEngine.UIElements;

namespace Rector.UI
{
    [UxmlElement("RectorButton")]
    public partial class RectorButton : VisualElement
    {
        [UxmlAttribute]
        public string Text
        {
            get => button.text;
            set => button.text = value;
        }

        readonly Button button = new();
        const string UssClassName = "rector-button";
        const string UssClassNameFocused = "rector-button--focused";
        const string UssClassNameHighlighted = "rector-button--highlighted";

        public RectorButton()
        {
            AddToClassList(UssClassName);
            AddToClassList(UssClassNameFocused);
            AddToClassList(UssClassNameHighlighted);

            EnableInClassList(UssClassNameFocused, false);
            EnableInClassList(UssClassNameHighlighted, false);

            Add(button);
        }


        public IDisposable Bind(RectorButtonState state)
        {
            return new CompositeDisposable(
                Observable.FromEvent(h => button.clicked += h, h => button.clicked -= h).Subscribe(_ => state.OnClick()),
                state.IsFocused.Subscribe(x => EnableInClassList(UssClassNameFocused, x)),
                state.IsHighlighted.Subscribe(x => EnableInClassList(UssClassNameHighlighted, x)),
                state.Text.Subscribe(x => button.text = x));
        }
    }
}
