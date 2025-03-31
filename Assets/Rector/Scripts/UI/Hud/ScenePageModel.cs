using System;
using System.Collections.Generic;
using R3;

namespace Rector.UI.Hud
{
    public sealed class ScenePageModel : IInitializable, IDisposable, IButtonListPageModel
    {
        readonly ButtonListPageView view;
        readonly BGSceneManager bgSceneManager;
        readonly ReactiveProperty<bool> isVisible = new(false);
        readonly List<RectorButtonState> buttons = new();
        readonly CompositeDisposable disposable = new();
        Action onExit;

        int index;

        public ScenePageModel(ButtonListPageView view, BGSceneManager bgSceneManager)
        {
            this.view = view;
            this.bgSceneManager = bgSceneManager;
        }

        void IInitializable.Initialize()
        {
            foreach (var scene in bgSceneManager.GetScenes())
            {
                var button = new RectorButtonState(scene, () => bgSceneManager.Load(scene));
                buttons.Add(button);
                bgSceneManager.CurrentScene.Subscribe(x => button.IsHighlighted.Value = x == scene).AddTo(disposable);
            }

            view.Bind(this).AddTo(disposable);
        }

        void IDisposable.Dispose() => disposable.Dispose();


        public void Enter(Action onExitCallback)
        {
            onExit = onExitCallback;
            isVisible.Value = true;
        }

        IEnumerable<RectorButtonState> IButtonListPageModel.GetButtons() => buttons;
        ReadOnlyReactiveProperty<bool> IButtonListPageModel.IsVisible => isVisible;

        void IButtonListPageModel.Submit() => buttons[index].OnClick();

        void IButtonListPageModel.Cancel()
        {
            isVisible.Value = false;
            onExit?.Invoke();
            onExit = null;
        }

        void IButtonListPageModel.Navigate(bool next)
        {
            buttons[index].IsFocused.Value = false;
            index += next ? 1 : -1;
            index = (index + buttons.Count) % buttons.Count;

            buttons[index].IsFocused.Value = true;
        }
    }
}
