using System;
using System.Collections.Generic;
using R3;

namespace Rector.UI.Hud
{
    public sealed class ScenePage : IInitializable, IDisposable
    {
        readonly ScenePageView view;
        readonly SceneManager sceneManager;
        public readonly ReactiveProperty<bool> IsVisible = new(false);
        public readonly List<RectorButtonState> Buttons = new();
        readonly CompositeDisposable disposable = new();
        Action onExit;

        int index;

        public ScenePage(ScenePageView view, SceneManager sceneManager)
        {
            this.view = view;
            this.sceneManager = sceneManager;
        }

        public void Initialize()
        {
            foreach (var scene in sceneManager.GetScenes())
            {
                var button = new RectorButtonState(scene, () => sceneManager.Load(scene));
                Buttons.Add(button);
                sceneManager.CurrentScene.Subscribe(x => button.IsHighlighted.Value = x == scene).AddTo(disposable);
            }

            view.Bind(this).AddTo(disposable);
        }

        public void Dispose() => disposable.Dispose();


        public void Enter(Action onExitCallback)
        {
            onExit = onExitCallback;
            IsVisible.Value = true;
        }

        public void Exit()
        {
            IsVisible.Value = false;
            onExit?.Invoke();
            onExit = null;
        }

        public void Navigate(bool next)
        {
            Buttons[index].IsFocused.Value = false;
            index += next ? 1 : -1;
            index = (index + Buttons.Count) % Buttons.Count;

            Buttons[index].IsFocused.Value = true;
        }

        public void Submit() => Buttons[index].OnClick();
    }
}
