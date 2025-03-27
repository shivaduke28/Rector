using System;
using System.Collections.Generic;
using R3;
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

    public sealed class ButtonListPageView
    {
        readonly VisualElement root;
        readonly UIInput uiInput;
        readonly VisualElement leftList;
        readonly SerialDisposable inputDisposable = new();

        IButtonListPageModel model;

        public ButtonListPageView(VisualElement root, UIInput uiInput)
        {
            this.root = root;
            this.uiInput = uiInput;
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

            var d = new CompositeDisposable(
                uiInput.Submit.Subscribe(_ => model.Submit()),
                uiInput.Cancel.Subscribe(_ => model.Cancel()),
                uiInput.Navigate.Where(i => i.y != 0)
                    .Subscribe(input => model.Navigate(input.y < 0))
            );

            leftList.Clear();
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
            root.style.display = DisplayStyle.None;
            inputDisposable.Disposable = null;
        }
    }
}
