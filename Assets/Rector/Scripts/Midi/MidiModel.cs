using System;
using System.Collections.Generic;
using Minis;
using R3;
using UnityEngine.InputSystem;

namespace Rector.Midi
{
    public sealed class MidiModel : IInitializable, IDisposable
    {
        readonly Subject<MidiNoteEvent> noteOn = new();
        readonly Subject<MidiNoteEvent> noteOff = new();
        readonly Subject<MidiCcEvent> controlChange = new();

        readonly MidiInputDeviceManager deviceManager;
        readonly CompositeDisposable disposable = new();

        // 接続中デバイスの登録簿。Minis の MidiDevice は (ポート x チャンネル) ごとに生えるので、
        // 同じポートのエントリが最大16個並ぶ。
        readonly Dictionary<MidiDevice, DeviceEntry> devices = new();

        readonly HashSet<string> selectedPorts = new();

        // 未選択ポートからの入力を握りつぶしたことを知らせる。毎イベント出すと洪水になるので1回だけ
        bool loggedIgnoredInput;

        public Observable<MidiNoteEvent> NoteOn => noteOn;
        public Observable<MidiNoteEvent> NoteOff => noteOff;
        public Observable<MidiCcEvent> ControlChange => controlChange;

        public MidiModel(MidiInputDeviceManager deviceManager)
        {
            this.deviceManager = deviceManager;
        }

        sealed class DeviceEntry
        {
            // MidiPortName.FromProduct は Substring なので、イベントごとに呼ぶと
            // スライダーを一本なぞるだけで数百回/秒の文字列確保になる。ここで1回だけ算出する
            public string PortName;
            public bool Enabled;
            public readonly HashSet<int> HeldNotes = new();
        }

        public void Initialize()
        {
            deviceManager.SelectedDevices.Subscribe(OnSelectionChanged).AddTo(disposable);

            InputSystem.onDeviceChange += OnDeviceChange;

            // Minis の MidiDevice は最初のメッセージ受信まで生えてこないが、
            // ドメインリロードをまたいで残っていることがあるので初期スキャンも行う
            foreach (var device in InputSystem.devices)
            {
                if (device is MidiDevice midiDevice)
                {
                    AddDevice(midiDevice);
                }
            }
        }

        void OnSelectionChanged(string[] portNames)
        {
            selectedPorts.Clear();
            foreach (var portName in portNames)
            {
                selectedPorts.Add(portName);
            }

            // 選択から外れたデバイスの押しっぱなしノートは、ここで解放しないと
            // Gate 出力が true のまま固着する。emit するとノード側のコードが同期で走るので、
            // 対象を集めてから流す
            List<MidiNoteEvent> released = null;

            foreach (var (device, entry) in devices)
            {
                var enabled = selectedPorts.Contains(entry.PortName);
                if (enabled == entry.Enabled) continue;

                if (!enabled && entry.HeldNotes.Count > 0)
                {
                    released ??= new List<MidiNoteEvent>();
                    foreach (var noteNumber in entry.HeldNotes)
                    {
                        released.Add(new MidiNoteEvent(device.channel, noteNumber, 0f));
                    }
                }

                // 有効化した側も捨てる。無効中の押下は noteOn を emit していないので、
                // 離したときの note-off も出してはいけない
                entry.HeldNotes.Clear();
                entry.Enabled = enabled;
            }

            if (released == null) return;
            foreach (var e in released)
            {
                noteOff.OnNext(e);
            }
        }

        void OnDeviceChange(InputDevice device, InputDeviceChange change)
        {
            if (device is not MidiDevice midiDevice) return;

            switch (change)
            {
                case InputDeviceChange.Added:
                case InputDeviceChange.Reconnected:
                    AddDevice(midiDevice);
                    break;
                case InputDeviceChange.Removed:
                case InputDeviceChange.Disconnected:
                    RemoveDevice(midiDevice);
                    break;
            }
        }

        void AddDevice(MidiDevice device)
        {
            if (devices.ContainsKey(device)) return;
            device.onWillNoteOn += HandleNoteOn;
            device.onWillNoteOff += HandleNoteOff;
            device.onWillControlChange += HandleControlChange;

            var portName = MidiPortName.FromProduct(device.description.product);
            devices.Add(device, new DeviceEntry
            {
                PortName = portName,
                Enabled = selectedPorts.Contains(portName)
            });
            RectorLogger.MidiInputDevice(device.description.product, device.channel, connected: true);
        }

        void RemoveDevice(MidiDevice device)
        {
            if (!devices.TryGetValue(device, out var entry)) return;
            Unsubscribe(device);
            devices.Remove(device);
            RectorLogger.MidiInputDevice(device.description.product, device.channel, connected: false);

            // 切断後は note-off が二度と来ないので、押しっぱなしのノートはここで解放する。
            // これをしないと Gate 出力が true のまま固着する。
            if (!entry.Enabled) return;
            foreach (var noteNumber in entry.HeldNotes)
            {
                noteOff.OnNext(new MidiNoteEvent(device.channel, noteNumber, 0f));
            }
        }

        void Unsubscribe(MidiDevice device)
        {
            device.onWillNoteOn -= HandleNoteOn;
            device.onWillNoteOff -= HandleNoteOff;
            device.onWillControlChange -= HandleControlChange;
        }

        // NOTE: HeldNotes の記帳は選択状態に関わらず行い、ゲートするのは OnNext だけ。
        // 記帳側をゲートすると、無効中に離した鍵が HeldNotes に残り続け、
        // 再び有効化して無効化するたびに押してもいないノートの note-off が飛ぶ。
        void HandleNoteOn(MidiNoteControl note, float velocity)
        {
            var entry = GetEntry(note);
            entry?.HeldNotes.Add(note.noteNumber);

            if (!ShouldEmit(entry)) return;
            noteOn.OnNext(new MidiNoteEvent(GetChannel(note), note.noteNumber, velocity));
        }

        void HandleNoteOff(MidiNoteControl note)
        {
            var entry = GetEntry(note);
            entry?.HeldNotes.Remove(note.noteNumber);

            if (!ShouldEmit(entry)) return;
            noteOff.OnNext(new MidiNoteEvent(GetChannel(note), note.noteNumber, 0f));
        }

        void HandleControlChange(MidiValueControl control, float value)
        {
            if (!ShouldEmit(GetEntry(control))) return;
            controlChange.OnNext(new MidiCcEvent(GetChannel(control), control.controlNumber, value));
        }

        DeviceEntry GetEntry(InputControl control)
            => control.device is MidiDevice device && devices.TryGetValue(device, out var entry) ? entry : null;

        // 未選択デバイスの取りこぼしを一度だけ知らせる副作用があるので、IsEnabled ではなくこの名前
        bool ShouldEmit(DeviceEntry entry)
        {
            if (entry is { Enabled: true }) return true;

            if (!loggedIgnoredInput)
            {
                loggedIgnoredInput = true;
                RectorLogger.MidiInputIgnored(entry?.PortName ?? "");
            }

            return false;
        }

        static int GetChannel(InputControl control) => (control.device as MidiDevice)?.channel ?? 0;

        public void Dispose()
        {
            InputSystem.onDeviceChange -= OnDeviceChange;
            foreach (var device in devices.Keys)
            {
                Unsubscribe(device);
            }

            devices.Clear();
            disposable.Dispose();
            noteOn.Dispose();
            noteOff.Dispose();
            controlChange.Dispose();
        }
    }
}
