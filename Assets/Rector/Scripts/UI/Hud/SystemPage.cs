using System;
using System.Collections.Generic;
using R3;
using UnityEngine;
using Screen = UnityEngine.Device.Screen;

namespace Rector.UI.Hud
{
    public sealed class SystemPage : IInitializable, IDisposable
    {
        public readonly List<RectorButtonState> Buttons;
        public readonly ReactiveProperty<bool> IsVisible = new(false);
        int index;

        readonly AudioInputDevicePage audioInputDevicePage;
        readonly DisplaySettingsPage displaySettingsPage;
        readonly SystemPageView view;
        Action onExit;
        IDisposable disposable;

        public SystemPage(
            AudioInputDevicePage audioInputDevicePage,
            DisplaySettingsPage displaySettingsPage,
            SystemPageView view)
        {
            this.audioInputDevicePage = audioInputDevicePage;
            this.displaySettingsPage = displaySettingsPage;
            this.view = view;
            Buttons = new List<RectorButtonState>
            {
                new("Audio Settings", ShowAudioSettings),
                new("Display settings", ShowDisplaySettings),
                new("Exit", ExitApplication),
            };
        }

        public void Initialize()
        {
            disposable = view.Bind(this);
        }

        public void Dispose() => disposable?.Dispose();

        public void Enter(Action onExitAction)
        {
            onExit = onExitAction;
            IsVisible.Value = true;
            index = 0;
            Buttons[index].IsFocused.Value = true;
        }

        void Resume()
        {
            IsVisible.Value = true;
        }

        void Exit()
        {
            Buttons[index].IsFocused.Value = false;
            IsVisible.Value = false;
            onExit?.Invoke();
        }

        public void Navigate(bool next)
        {
            Buttons[index].IsFocused.Value = false;
            index += next ? 1 : -1;
            index = (index + Buttons.Count) % Buttons.Count;

            Buttons[index].IsFocused.Value = true;
        }

        public void Submit() => Buttons[index].OnClick();

        public void Cancel() => Exit();

        void ShowAudioSettings()
        {
            IsVisible.Value = false;
            audioInputDevicePage.Enter(Resume);
        }

        void ShowDisplaySettings()
        {
            IsVisible.Value = false;
            displaySettingsPage.Enter(Resume);
        }

        void ExitApplication()
        {
            // TODO: Add a confirmation dialog
#if UNITY_EDITOR
            UnityEditor.EditorApplication.ExitPlaymode();
#else
            UnityEngine.Application.Quit();
#endif
        }
    }
}
