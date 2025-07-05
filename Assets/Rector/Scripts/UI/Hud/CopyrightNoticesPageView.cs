using System;
using R3;
using UnityEngine.UIElements;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Rector.UI.Hud
{
    public sealed class CopyrightNoticesPageView : IUIInputHandler
    {
        readonly VisualElement root;
        readonly Label label;
        readonly UIInputAction uiInputAction;
        CopyrightNoticesPageModel model;
        readonly SerialDisposable inputDisposable = new();
        readonly SerialDisposable scrollDisposable = new();
        const float ScrollSpeed = 50f;

        public CopyrightNoticesPageView(VisualElement root, UIInputAction uiInputAction)
        {
            this.root = root;
            this.uiInputAction = uiInputAction;
            label = root.Q<Label>("copyright-notices-label");
        }

        public IDisposable Bind(CopyrightNoticesPageModel model)
        {
            this.model = model;
            return model.IsVisible.Subscribe(visible =>
            {
                if (visible)
                    Show().Forget();
                else
                    Hide();
            });
        }

        void Hide()
        {
            root.style.display = DisplayStyle.None;
            inputDisposable.Disposable = null;
            scrollDisposable.Disposable = null;
            label.text = "";
            uiInputAction.Unregister(this);
        }

        async UniTaskVoid Show()
        {
            root.style.display = DisplayStyle.Flex;
            label.text = await model.LoadCopyrightNoticesAsync();
            label.transform.position = new Vector3(0, 0, 0);
            uiInputAction.Register(this);

            scrollDisposable.Disposable = Observable.Timer(TimeSpan.FromSeconds(1))
                .SelectMany(_ => Observable.EveryUpdate())
                .Subscribe(_ =>
                {
                    var pos = label.transform.position;
                    pos.y -= ScrollSpeed * Time.deltaTime;
                    pos.y = Mathf.Clamp(pos.y, root.resolvedStyle.height - label.resolvedStyle.height, 0f);
                    label.transform.position = pos;
                });
        }

        void MoveLabel(float y)
        {
            var pos = label.transform.position;
            pos.y += y * 10;
            pos.y = Mathf.Clamp(pos.y, root.resolvedStyle.height - label.resolvedStyle.height, 0f);
            label.transform.position = pos;
        }

        void IUIInputHandler.OnNavigate(Vector2 value)
        {
            if (value.y != 0)
            {
                MoveLabel(value.y);
            }
        }

        void IUIInputHandler.OnSubmit()
        {
        }

        void IUIInputHandler.OnCancel()
        {
            model.Cancel();
        }
    }
}
