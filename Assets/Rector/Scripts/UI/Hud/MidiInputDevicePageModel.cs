using System;
using System.Collections.Generic;
using System.Linq;
using R3;
using Rector.Midi;

namespace Rector.UI.Hud
{
    public sealed class MidiInputDevicePageModel : IInitializable, IDisposable, IButtonListPageModel
    {
        readonly ReactiveProperty<bool> isVisible = new(false);
        readonly MidiInputDeviceManager midiInputDeviceManager;
        readonly ButtonListPageView view;
        readonly List<RectorButtonState> buttons = new();
        readonly SerialDisposable enterDisposable = new();
        readonly CompositeDisposable disposable = new();
        Action onExit;

        ReadOnlyReactiveProperty<bool> IButtonListPageModel.IsVisible => isVisible;
        IEnumerable<RectorButtonState> IButtonListPageModel.GetButtons() => buttons;

        int index;

        public MidiInputDevicePageModel(MidiInputDeviceManager midiInputDeviceManager,
            ButtonListPageView view)
        {
            this.midiInputDeviceManager = midiInputDeviceManager;
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
            foreach (var portName in midiInputDeviceManager.GetInputDevices().OrderBy(x => x))
            {
                var button = new RectorButtonState(portName, () => midiInputDeviceManager.Toggle(portName));
                buttons.Add(button);

                // トグルしてもリストは組み直さないので、ハイライトだけが反応してフォーカスは動かない
                midiInputDeviceManager.SelectedDevices
                    .Subscribe(selected => button.IsHighlighted.Value = Array.IndexOf(selected, portName) >= 0)
                    .AddTo(d);
            }

            // MIDI 機器を挿していないのは日常的に起きる。真っ白なページだと壊れたのと区別が付かない
            if (buttons.Count == 0)
            {
                buttons.Add(new RectorButtonState("No MIDI Devices", () => { }));
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
