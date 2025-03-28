using System;
using System.Collections.Generic;
using System.Linq;
using R3;
using UnityEngine;
using UnityEngine.UIElements;

namespace Rector.UI.Hud
{
    public interface IButtonListPageModel
    {
        void Submit();
        void Cancel();
        void Navigate(bool up);
        IEnumerable<RectorButtonState> GetButtons();
        ReadOnlyReactiveProperty<bool> IsVisible { get; }
    }

    public sealed class ButtonListPageView : IUIInputHandler
    {
        readonly VisualElement root;
        readonly UIInputAction uiInputAction;
        readonly VisualElement leftList;
        readonly SerialDisposable inputDisposable = new();

        IButtonListPageModel model;

        public ButtonListPageView(VisualElement root, UIInputAction uiInputAction)
        {
            this.root = root;
            this.uiInputAction = uiInputAction;
            leftList = root.Q<VisualElement>("left-list");
        }

        public IDisposable Bind(IButtonListPageModel page)
        {
            model = page;
            var disposable = new CompositeDisposable();

            page.IsVisible.Subscribe(visible =>
            {
                if (visible)
                    Show();
                else
                    Hide();
            }).AddTo(disposable);
            inputDisposable.AddTo(disposable);
            return disposable;
        }

        void Show()
        {
            root.style.display = DisplayStyle.Flex;
            uiInputAction.Register(this);

            leftList.Clear();
            var d = new CompositeDisposable();
            foreach (var button in model.GetButtons())
            {
                var rectorButton = new RectorButton();
                rectorButton.Bind(button).AddTo(d);
                leftList.Add(rectorButton);
            }

            inputDisposable.Disposable = d;
        }

        void Hide()
        {
            uiInputAction.Unregister(this);
            root.style.display = DisplayStyle.None;
            inputDisposable.Disposable = null;
        }

        void IUIInputHandler.OnNavigate(Vector2 value)
        {
            if (value.y > 0)
            {
                model.Navigate(false);
            }
            else if (value.y < 0)
            {
                model.Navigate(true);
            }
        }

        void IUIInputHandler.OnSubmit()
        {
            model.Submit();
        }

        void IUIInputHandler.OnCancel()
        {
            model.Cancel();
        }
    }
}
