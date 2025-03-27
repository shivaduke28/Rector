using System;
using System.Collections.Generic;
using R3;
using UnityEngine;

namespace Rector.UI.Hud
{
    public sealed class DisplaySettingsPage : IInitializable, IDisposable, IButtonListPageModel
    {
        readonly ButtonListPageView view;
        readonly ReactiveProperty<bool> isVisible = new(false);
        ReadOnlyReactiveProperty<bool> IButtonListPageModel.IsVisible => isVisible;

        Action onExit;

        int index;

        readonly List<RectorButtonState> buttons = new();

        IDisposable disposable;

        public DisplaySettingsPage(ButtonListPageView view)
        {
            this.view = view;
        }

        public void Initialize()
        {
            disposable = view.Bind(this);
        }

        public void Dispose() => disposable?.Dispose();

        public void Enter(Action onExitAction)
        {
            index = 0;
            onExit = onExitAction;

            buttons.Clear();
            buttons.Add(new RectorButtonState(FullScreenMode.ExclusiveFullScreen.ToString(), () => ChangeFullScreenMode(FullScreenMode.ExclusiveFullScreen)));
            buttons.Add(new RectorButtonState(FullScreenMode.FullScreenWindow.ToString(), () => ChangeFullScreenMode(FullScreenMode.FullScreenWindow)));
            buttons.Add(new RectorButtonState(FullScreenMode.MaximizedWindow.ToString(), () => ChangeFullScreenMode(FullScreenMode.MaximizedWindow)));
            buttons.Add(new RectorButtonState(FullScreenMode.Windowed.ToString(), () => ChangeFullScreenMode(FullScreenMode.Windowed)));

            var resolutions = Screen.resolutions;
            foreach (var resolution in resolutions)
            {
                buttons.Add(new RectorButtonState($"{resolution.width} x {resolution.height}", () => UpdateResolution(resolution)));
            }

            if (buttons.Count > 0)
            {
                buttons[0].IsFocused.Value = true;
            }

            isVisible.Value = true;
        }

        void IButtonListPageModel.Submit()
        {
            buttons[index].OnClick();
        }

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

        IEnumerable<RectorButtonState> IButtonListPageModel.GetButtons() => buttons;

        static void ChangeFullScreenMode(FullScreenMode fullScreenMode)
        {
            Screen.SetResolution(Screen.width, Screen.height, fullScreenMode, new RefreshRate
            {
                numerator = 60,
                denominator = 1
            });
            RectorLogger.Resolution(Screen.width, Screen.height, fullScreenMode);
        }

        void UpdateResolution(Resolution resolution)
        {
            Screen.SetResolution(resolution.width, resolution.height, Screen.fullScreenMode, new RefreshRate
            {
                numerator = 60,
                denominator = 1
            });
            RectorLogger.Resolution(resolution.width, resolution.height, Screen.fullScreenMode);
        }
    }
}
