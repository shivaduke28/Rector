using System;
using System.Collections.Generic;
using System.Linq;
using R3;
using Rector.Midi;
using Rector.UI.Settings;

namespace Rector.UI.Hud
{
    public sealed class MidiInputDevicePageModel : IInitializable, IDisposable, ISettingsPageModel
    {
        // 左がOff、右がOn。1デバイス1行で、左右キーで受信の入切を変える。
        static readonly string[] SwitchOptions = { "Off", "On" };

        readonly ReactiveProperty<bool> isVisible = new(false);
        readonly MidiInputDeviceManager midiInputDeviceManager;
        readonly SettingsPageView view;
        readonly List<ISettingRow> rows = new();
        readonly SerialDisposable enterDisposable = new();
        readonly CompositeDisposable disposable = new();
        Action onExit;

        ReadOnlyReactiveProperty<bool> ISettingsPageModel.IsVisible => isVisible;
        IReadOnlyList<ISettingRow> ISettingsPageModel.GetRows() => rows;

        public MidiInputDevicePageModel(MidiInputDeviceManager midiInputDeviceManager,
            SettingsPageView view)
        {
            this.midiInputDeviceManager = midiInputDeviceManager;
            this.view = view;
        }

        public void Enter(Action onExitAction)
        {
            RefreshDevices();

            onExit = onExitAction;
            isVisible.Value = true;
        }

        void IInitializable.Initialize()
        {
            view.Bind(this).AddTo(disposable);
        }

        void RefreshDevices()
        {
            rows.Clear();
            var d = new CompositeDisposable();
            var selectedDevices = midiInputDeviceManager.SelectedDevices;
            foreach (var portName in midiInputDeviceManager.GetInputDevices().OrderBy(x => x))
            {
                var name = portName;
                var row = new StepperRowState(
                    name,
                    SwitchOptions,
                    Array.IndexOf(selectedDevices.CurrentValue, name) >= 0 ? 1 : 0,
                    i => midiInputDeviceManager.SetSelected(name, i == 1));
                rows.Add(row);

                // 入切してもリストは組み直さないので、値だけが追従してフォーカスは動かない
                selectedDevices
                    .Subscribe(selected => row.SetIndexWithoutNotify(Array.IndexOf(selected, name) >= 0 ? 1 : 0))
                    .AddTo(d);
            }

            // MIDI 機器を挿していないのは日常的に起きる。真っ白なページだと壊れたのと区別が付かない
            if (rows.Count == 0)
            {
                rows.Add(new StepperRowState("MIDI Device", new[] { "None" }, 0, _ => { }));
            }

            enterDisposable.Disposable = d;
        }

        void ISettingsPageModel.Cancel()
        {
            enterDisposable.Disposable = null;
            isVisible.Value = false;
            onExit?.Invoke();
            onExit = null;
        }

        void IDisposable.Dispose()
        {
            enterDisposable.Dispose();
            disposable.Dispose();
        }
    }
}
