using System;
using R3;
using UnityEngine;
using UnityEngine.UIElements;

namespace Rector.UI.Graphs
{
    [UxmlElement("HoldGuide")]
    public sealed partial class HoldGuideView : VisualElement
    {
        const string UssClassName = "rector-hold-guide";
        const string UssFillClassName = UssClassName + "__fill";
        const string UssFillFilledClassName = UssFillClassName + "--filled";

        readonly VisualElement fill;

        public HoldGuideView()
        {
            AddToClassList(UssClassName);
            fill = new VisualElement();
            fill.AddToClassList(UssFillClassName);
            fill.AddToClassList(UssFillFilledClassName);
            fill.EnableInClassList(UssFillFilledClassName, false);

            Add(fill);
        }

        public IDisposable Bind(HoldGuideModel model)
        {
            return new CompositeDisposable(
                model.Position.Subscribe(SetPosition),
                model.Visible.Subscribe(SetVisible)
            );
        }

        void SetPosition(Vector2 position)
        {
            style.left = position.x;
            style.top = position.y;
        }

        void SetVisible(bool value)
        {
            fill.EnableInClassList(UssFillFilledClassName, value);
            style.visibility = value ? Visibility.Visible : Visibility.Hidden;
        }
    }
}
