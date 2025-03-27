using System;
using System.Collections.Generic;
using R3;

namespace Rector.UI.Hud
{
    public sealed class SystemPageModel : IInitializable, IDisposable, IButtonListPageModel
    {
        readonly RectorButtonState[] buttons;
        readonly ReactiveProperty<bool> isVisible = new(false);
        int index;

        readonly AudioInputDevicePageModel audioInputDevicePageModel;
        readonly DisplaySettingsPageModel displaySettingsPageModel;
        readonly ButtonListPageView view;
        Action onExit;
        IDisposable disposable;

        public SystemPageModel(
            AudioInputDevicePageModel audioInputDevicePageModel,
            DisplaySettingsPageModel displaySettingsPageModel,
            ButtonListPageView view)
        {
            this.audioInputDevicePageModel = audioInputDevicePageModel;
            this.displaySettingsPageModel = displaySettingsPageModel;
            this.view = view;
            buttons = new RectorButtonState[]
            {
                new("Audio Settings", ShowAudioSettings),
                new("Display settings", ShowDisplaySettings),
                new("Exit", ExitApplication),
            };
        }

        void IInitializable.Initialize()
        {
            disposable = view.Bind(this);
        }

        void IDisposable.Dispose() => disposable?.Dispose();

        public void Enter(Action onExitAction)
        {
            onExit = onExitAction;
            isVisible.Value = true;
            index = 0;
            buttons[index].IsFocused.Value = true;
        }

        void Resume()
        {
            isVisible.Value = true;
        }


        IEnumerable<RectorButtonState> IButtonListPageModel.GetButtons() => buttons;

        ReadOnlyReactiveProperty<bool> IButtonListPageModel.IsVisible => isVisible;

        void IButtonListPageModel.Submit() => buttons[index].OnClick();

        void IButtonListPageModel.Cancel()
        {
            buttons[index].IsFocused.Value = false;
            isVisible.Value = false;
            onExit?.Invoke();
        }

        void IButtonListPageModel.Navigate(bool next)
        {
            buttons[index].IsFocused.Value = false;
            index += next ? 1 : -1;
            index = (index + buttons.Length) % buttons.Length;

            buttons[index].IsFocused.Value = true;
        }


        void ShowAudioSettings()
        {
            isVisible.Value = false;
            audioInputDevicePageModel.Enter(Resume);
        }

        void ShowDisplaySettings()
        {
            isVisible.Value = false;
            displaySettingsPageModel.Enter(Resume);
        }

        void ExitApplication()
        {
            // TODO: Add a confirmation dialog?
#if UNITY_EDITOR
            UnityEditor.EditorApplication.ExitPlaymode();
#else
            UnityEngine.Application.Quit();
#endif
        }
    }
}
