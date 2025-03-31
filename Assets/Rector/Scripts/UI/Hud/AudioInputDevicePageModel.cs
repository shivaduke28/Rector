using System;
using System.Collections.Generic;
using System.Linq;
using R3;
using Rector.Audio;

namespace Rector.UI.Hud
{
    public sealed class AudioInputDevicePageModel : IInitializable, IDisposable, IButtonListPageModel
    {
        readonly ReactiveProperty<bool> isVisible = new(false);
        readonly AudioInputDeviceManager audioInputDeviceManager;
        readonly ButtonListPageView view;
        readonly List<RectorButtonState> buttons = new();
        readonly SerialDisposable enterDisposable = new();
        readonly CompositeDisposable disposable = new();
        Action onExit;

        ReadOnlyReactiveProperty<bool> IButtonListPageModel.IsVisible => isVisible;
        IEnumerable<RectorButtonState> IButtonListPageModel.GetButtons() => buttons;

        int index;

        public AudioInputDevicePageModel(AudioInputDeviceManager audioInputDeviceManager,
            ButtonListPageView view)
        {
            this.audioInputDeviceManager = audioInputDeviceManager;
            this.view = view;
        }

        public void Enter(Action onExitAction)
        {
            RefreshDevices();

            index = 0;
            if (buttons.Count > 0)
            {
                buttons[index].IsFocused.Value = true;
            }

            onExit = onExitAction;
            isVisible.Value = true;
        }

        void IInitializable.Initialize()
        {
            view.Bind(this).AddTo(disposable);
        }

        void RefreshDevices()
        {
            buttons.Clear();
            var d = new CompositeDisposable();
            foreach (var inputDevice in audioInputDeviceManager.GetInputDevices().OrderBy(x => x.Name))
            {
                var button = new RectorButtonState(inputDevice.Name, () => audioInputDeviceManager.SwitchDevice(inputDevice));
                buttons.Add(button);
                audioInputDeviceManager.CurrentInputDevice
                    .Where(x => x.IsValid)
                    .Subscribe(currentDevice => button.IsHighlighted.Value = currentDevice.Equals(inputDevice))
                    .AddTo(d);
            }

            enterDisposable.Disposable = d;
        }

        void IButtonListPageModel.Submit()
        {
            if (buttons.Count == 0) return;
            buttons[index].OnClick();
        }

        void IButtonListPageModel.Cancel()
        {
            enterDisposable.Disposable = null;
            isVisible.Value = false;
            onExit?.Invoke();
        }

        void IButtonListPageModel.Navigate(bool next)
        {
            if (buttons.Count == 0) return;
            buttons[index].IsFocused.Value = false;

            index += next ? 1 : -1;
            index = (index + buttons.Count) % buttons.Count;

            buttons[index].IsFocused.Value = true;
        }

        void IDisposable.Dispose()
        {
            enterDisposable.Dispose();
            disposable.Dispose();
        }
    }
}
