using System;
using System.Collections.Generic;
using R3;
using UnityEngine;

namespace Rector.UI.Hud
{
    public sealed class DisplaySettingsPage : IInitializable, IDisposable
    {
        readonly DisplaySettingsPageView view;
        public readonly ReactiveProperty<bool> IsVisible = new(false);
        Action onExit;

        int index;

        public readonly List<RectorButtonState> Buttons = new();

        IDisposable disposable;

        public DisplaySettingsPage(DisplaySettingsPageView view)
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

            Buttons.Clear();

            Buttons.Add(new RectorButtonState(FullScreenMode.ExclusiveFullScreen.ToString(), () => ChangeFullScreenMode(FullScreenMode.ExclusiveFullScreen)));
            Buttons.Add(new RectorButtonState(FullScreenMode.FullScreenWindow.ToString(), () => ChangeFullScreenMode(FullScreenMode.FullScreenWindow)));
            Buttons.Add(new RectorButtonState(FullScreenMode.MaximizedWindow.ToString(), () => ChangeFullScreenMode(FullScreenMode.MaximizedWindow)));
            Buttons.Add(new RectorButtonState(FullScreenMode.Windowed.ToString(), () => ChangeFullScreenMode(FullScreenMode.Windowed)));

            var resolutions = Screen.resolutions;
            foreach (var resolution in resolutions)
            {
                Buttons.Add(new RectorButtonState($"{resolution.width} x {resolution.height}", () => UpdateResolution(resolution)));
            }

            if (Buttons.Count > 0)
            {
                Buttons[0].IsFocused.Value = true;
            }

            IsVisible.Value = true;
        }

        public void Exit()
        {
            IsVisible.Value = false;
            onExit?.Invoke();
            onExit = null;
        }

        public void Submit()
        {
            Buttons[index].OnClick();
        }

        public void Navigate(bool next)
        {
            Buttons[index].IsFocused.Value = false;

            index += next ? 1 : -1;
            index = (index + Buttons.Count) % Buttons.Count;

            Buttons[index].IsFocused.Value = true;
        }

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
