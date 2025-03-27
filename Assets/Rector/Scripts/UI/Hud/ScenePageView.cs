using System;
using R3;
using UnityEngine.UIElements;

namespace Rector.UI.Hud
{
    public sealed class ScenePageView
    {
        readonly VisualElement root;
        readonly UIInput uiInput;
        readonly VisualElement leftList;
        readonly SerialDisposable inputDisposable = new();
        ScenePage model;

        public ScenePageView(VisualElement root, UIInput uiInput)
        {
            this.root = root;
            this.uiInput = uiInput;
            leftList = root.Q<VisualElement>("left-list");
        }

        public IDisposable Bind(ScenePage page)
        {
            model = page;
            var disposable = new CompositeDisposable();
            foreach (var button in page.Buttons)
            {
                var rectorButton = new RectorButton();
                rectorButton.Bind(button).AddTo(disposable);
                leftList.Add(rectorButton);
            }
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
            inputDisposable.Disposable = new CompositeDisposable(
                uiInput.Submit.Subscribe(_ => model.Submit()),
                uiInput.Cancel.Subscribe(_ => model.Exit()),
                uiInput.Navigate.Where(i => i.y != 0)
                    .Subscribe(input => model.Navigate(input.y < 0))
            );
        }

        void Hide()
        {
            root.style.display = DisplayStyle.None;
            inputDisposable.Disposable = null;
        }
    }
}
