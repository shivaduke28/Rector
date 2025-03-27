using System;
using R3;
using UnityEngine;
using UnityEngine.UIElements;

namespace Rector.UI.Hud
{
    public sealed class HudFrameView
    {
        readonly VisualElement header;
        readonly VisualElement sideLeft;
        readonly VisualElement sideRight;
        readonly VisualElement footer;

        public HudFrameView(VisualElement hudRoot)
        {
            header = hudRoot.Q<VisualElement>("header");
            sideLeft = hudRoot.Q<VisualElement>("side-left");
            sideRight = hudRoot.Q<VisualElement>("side-right");
            footer = hudRoot.Q<VisualElement>("footer");
        }

        public IDisposable Bind(ReadOnlyReactiveProperty<Color> color)
        {
            return color.Subscribe(c =>
            {
                header.style.backgroundColor = c;
                sideLeft.style.backgroundColor = c;
                sideRight.style.backgroundColor = c;
                footer.style.backgroundColor = c;
            });
        }
    }
}
