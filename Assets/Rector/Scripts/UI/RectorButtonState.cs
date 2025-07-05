using System;
using R3;

namespace Rector.UI
{
    public sealed class RectorButtonState
    {
        public readonly ReactiveProperty<bool> IsFocused = new(false);
        public readonly ReactiveProperty<bool> IsHighlighted = new(false);
        public readonly ReactiveProperty<string> Text = new(string.Empty);

        public readonly Action OnClick;

        public RectorButtonState(string text, Action onClick)
        {
            Text.Value = text;
            OnClick = onClick;
        }


        public RectorButtonState(Action onClick)
        {
            OnClick = onClick;
        }
    }
}
