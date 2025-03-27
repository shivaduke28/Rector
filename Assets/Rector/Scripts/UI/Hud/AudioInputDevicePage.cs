using System;
using System.Collections.Generic;
using System.Linq;
using R3;
using Rector.Audio;

namespace Rector.UI.Hud
{
    public sealed class AudioInputDevicePage : IInitializable, IDisposable
    {
        public readonly ReactiveProperty<bool> IsVisible = new(false);
        readonly AudioInputDeviceManager audioInputDeviceManager;
        readonly AudioInputDevicePageView view;
        public readonly List<RectorButtonState> Buttons = new();
        readonly SerialDisposable enterDisposable = new();
        readonly CompositeDisposable disposable = new();
        Action onExit;

        int index;

        public AudioInputDevicePage(AudioInputDeviceManager audioInputDeviceManager,
            AudioInputDevicePageView view)
        {
            this.audioInputDeviceManager = audioInputDeviceManager;
            this.view = view;
        }

        public void Enter(Action onExitAction)
        {
            RefreshDevices();

            if (Buttons.Count > 0)
            {
                index = 0;
                Buttons[index].IsFocused.Value = true;
            }
            else
            {
                index = -1;
            }

            onExit = onExitAction;
            IsVisible.Value = true;
        }

        public void Exit()
        {
            enterDisposable.Disposable = null;
            IsVisible.Value = false;
            onExit?.Invoke();
        }

        public void Initialize()
        {
            view.Bind(this).AddTo(disposable);
        }

        void RefreshDevices()
        {
            Buttons.Clear();
            var deviceDisposable = new CompositeDisposable();
            foreach (var inputDevice in audioInputDeviceManager.GetInputDevices().OrderBy(x => x.Name))
            {
                var button = new RectorButtonState(inputDevice.Name, () => audioInputDeviceManager.Switch(inputDevice));
                Buttons.Add(button);
                audioInputDeviceManager.CurrentInputDevice
                    .Where(x => x.IsValid)
                    .Subscribe(currentDevice => button.IsHighlighted.Value = currentDevice.Equals(inputDevice))
                    .AddTo(deviceDisposable);
            }

            enterDisposable.Disposable = deviceDisposable;
        }

        public void Navigate(bool next)
        {
            if (index == -1) return;
            Buttons[index].IsFocused.Value = false;

            index += next ? 1 : -1;
            index = (index + Buttons.Count) % Buttons.Count;

            Buttons[index].IsFocused.Value = true;
        }

        public void Submit() => Buttons[index].OnClick();

        public void Dispose()
        {
            enterDisposable.Dispose();
            disposable.Dispose();
        }
    }
}
