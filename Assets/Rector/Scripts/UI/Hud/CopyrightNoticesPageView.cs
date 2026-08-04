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

        // resolvedStyle はレイアウト解決後にしか更新されないため、
        // 読み戻さずに済むようスクロール位置を保持する。x は常に 0。
        float scrollY;

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
            // ここではクランプしない。text 代入直後で resolvedStyle.height が未確定のため。
            scrollY = 0f;
            label.style.translate = Vector2.zero;
            uiInputAction.Register(this);

            scrollDisposable.Disposable = Observable.Timer(TimeSpan.FromSeconds(1))
                .SelectMany(_ => Observable.EveryUpdate())
                .Subscribe(_ => SetScrollY(scrollY - ScrollSpeed * Time.deltaTime));
        }

        void SetScrollY(float y)
        {
            scrollY = Mathf.Clamp(y, root.resolvedStyle.height - label.resolvedStyle.height, 0f);
            label.style.translate = new Vector2(0f, scrollY);
        }

        void MoveLabel(float y)
        {
            SetScrollY(scrollY + y * 10);
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
