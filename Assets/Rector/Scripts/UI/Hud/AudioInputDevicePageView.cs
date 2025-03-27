using System;
using R3;
using UnityEngine.UIElements;

namespace Rector.UI.Hud
{
    public sealed class AudioInputDevicePageView
    {
        readonly VisualElement root;
        readonly UIInput uiInput;
        readonly VisualElement leftList;
        readonly SerialDisposable inputDisposable = new();
        AudioInputDevicePage model;

        public AudioInputDevicePageView(VisualElement root, UIInput uiInput)
        {
            this.root = root;
            this.uiInput = uiInput;
            leftList = root.Q<VisualElement>("left-list");
        }

        public IDisposable Bind(AudioInputDevicePage page)
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
                uiInput.Cancel.Subscribe(_ => model.Exit()),
                uiInput.Navigate.Where(i => i.y != 0)
                    .Subscribe(input => model.Navigate(input.y < 0))
            );

            leftList.Clear();
            foreach (var button in model.Buttons)
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
