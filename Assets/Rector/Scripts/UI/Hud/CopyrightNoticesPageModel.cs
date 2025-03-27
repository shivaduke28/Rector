using System;
using Cysharp.Threading.Tasks;
using R3;
using UnityEngine;

namespace Rector.UI.Hud
{
    public sealed class CopyrightNoticesPageModel : IInitializable, IDisposable
    {
        readonly ReactiveProperty<bool> isVisible = new(false);
        readonly CopyrightNoticesPageView view;

        IDisposable disposable;

        Action onExitCallback;
        public ReadOnlyReactiveProperty<bool> IsVisible => isVisible;

        public CopyrightNoticesPageModel(CopyrightNoticesPageView view)
        {
            this.view = view;
        }

        void IInitializable.Initialize()
        {
            disposable = view.Bind(this);
        }

        void IDisposable.Dispose()
        {
            disposable?.Dispose();
        }

        public void Enter(Action onExit)
        {
            onExitCallback = onExit;
            isVisible.Value = true;
        }

        public void Cancel()
        {
            isVisible.Value = false;
            onExitCallback?.Invoke();
        }

        public async UniTask<string> LoadCopyrightNoticesAsync()
        {
            string text;
            var obj = await Resources.LoadAsync<TextAsset>("CopyrightNotices");
            if (obj is TextAsset textAsset)
            {
                text = textAsset.text;
                Resources.UnloadAsset(textAsset);
            }
            else
            {
                text = "Failed to load CopyrightNotices.txt";
            }

            return text;
        }
    }
}
