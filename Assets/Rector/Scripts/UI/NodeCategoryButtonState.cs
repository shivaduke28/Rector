using System;
using R3;
using UnityEngine;

namespace Rector.UI
{
    public sealed class NodeCategoryButtonState
    {
        public readonly ReactiveProperty<bool> IsFocused = new(false);
        public readonly ReactiveProperty<bool> IsHighlighted = new(false);
        public readonly ReactiveProperty<string> Text = new(string.Empty);
        public readonly ReactiveProperty<Texture2D> Icon = new(null);

        public readonly Action OnClick;

        public NodeCategoryButtonState(string text, Texture2D icon, Action onClick)
        {
            Text.Value = text;
            Icon.Value = icon;
            OnClick = onClick;
        }
    }
}