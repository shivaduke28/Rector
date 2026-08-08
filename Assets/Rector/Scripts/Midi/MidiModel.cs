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

        // 接続中デバイスの登録簿を兼ねる。値は押しっぱなしのノート番号。
        readonly Dictionary<MidiDevice, HashSet<int>> heldNotes = new();

        public Observable<MidiNoteEvent> NoteOn => noteOn;
        public Observable<MidiNoteEvent> NoteOff => noteOff;
        public Observable<MidiCcEvent> ControlChange => controlChange;

        public void Initialize()
        {
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
            if (heldNotes.ContainsKey(device)) return;
            device.onWillNoteOn += HandleNoteOn;
            device.onWillNoteOff += HandleNoteOff;
            device.onWillControlChange += HandleControlChange;
            heldNotes.Add(device, new HashSet<int>());
            RectorLogger.MidiInputDevice(device.description.product, device.channel, connected: true);
        }

        void RemoveDevice(MidiDevice device)
        {
            if (!heldNotes.TryGetValue(device, out var held)) return;
            Unsubscribe(device);
            heldNotes.Remove(device);
            RectorLogger.MidiInputDevice(device.description.product, device.channel, connected: false);

            // 切断後は note-off が二度と来ないので、押しっぱなしのノートはここで解放する。
            // これをしないと Gate 出力が true のまま固着する。
            foreach (var noteNumber in held)
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

        void HandleNoteOn(MidiNoteControl note, float velocity)
        {
            if (note.device is MidiDevice device && heldNotes.TryGetValue(device, out var held))
            {
                held.Add(note.noteNumber);
            }

            noteOn.OnNext(new MidiNoteEvent(GetChannel(note), note.noteNumber, velocity));
        }

        void HandleNoteOff(MidiNoteControl note)
        {
            if (note.device is MidiDevice device && heldNotes.TryGetValue(device, out var held))
            {
                held.Remove(note.noteNumber);
            }

            noteOff.OnNext(new MidiNoteEvent(GetChannel(note), note.noteNumber, 0f));
        }

        void HandleControlChange(MidiValueControl control, float value)
        {
            controlChange.OnNext(new MidiCcEvent(GetChannel(control), control.controlNumber, value));
        }

        static int GetChannel(InputControl control) => (control.device as MidiDevice)?.channel ?? 0;

        public void Dispose()
        {
            InputSystem.onDeviceChange -= OnDeviceChange;
            foreach (var device in heldNotes.Keys)
            {
                Unsubscribe(device);
            }

            heldNotes.Clear();
            noteOn.Dispose();
            noteOff.Dispose();
            controlChange.Dispose();
        }
    }
}
