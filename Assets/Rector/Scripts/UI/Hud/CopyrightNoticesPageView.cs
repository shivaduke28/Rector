using System;
using R3;
using UnityEngine.UIElements;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Rector.UI.Hud
{
    public sealed class CopyrightNoticesPageView
    {
        readonly VisualElement root;
        readonly Label label;
        readonly UIInput uiInput;
        CopyrightNoticesPageModel model;
        readonly SerialDisposable inputDisposable = new();

        public CopyrightNoticesPageView(VisualElement root, UIInput uiInput)
        {
            this.root = root;
            label = root.Q<Label>("copyright-notices-label");
            this.uiInput = uiInput;
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
            label.text = "";
        }

        async UniTaskVoid Show()
        {
            root.style.display = DisplayStyle.Flex;
            label.text = await model.LoadCopyrightNoticesAsync();
            label.transform.position = new Vector3(0, 0, 0);

            inputDisposable.Disposable = new CompositeDisposable(
                uiInput.Cancel.Subscribe(_ => model.Cancel()),
                uiInput.Navigate.Where(i => i.y != 0)
                    .Subscribe(input => MoveLabel(input.y))
            );
        }

        void MoveLabel(float y)
        {
            var pos = label.transform.position;
            pos.y += y * 10;
            pos.y = Mathf.Clamp(pos.y, root.resolvedStyle.height - label.resolvedStyle.height, 0f);
            label.transform.position = pos;
        }
    }
}
