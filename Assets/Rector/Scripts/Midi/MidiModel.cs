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
        readonly List<MidiDevice> devices = new();

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
            if (devices.Contains(device)) return;
            device.onWillNoteOn += HandleNoteOn;
            device.onWillNoteOff += HandleNoteOff;
            device.onWillControlChange += HandleControlChange;
            devices.Add(device);
            RectorLogger.MidiInputDevice(device.description.product, device.channel, connected: true);
        }

        void RemoveDevice(MidiDevice device)
        {
            if (!devices.Remove(device)) return;
            device.onWillNoteOn -= HandleNoteOn;
            device.onWillNoteOff -= HandleNoteOff;
            device.onWillControlChange -= HandleControlChange;
            RectorLogger.MidiInputDevice(device.description.product, device.channel, connected: false);
        }

        void HandleNoteOn(MidiNoteControl note, float velocity)
        {
            noteOn.OnNext(new MidiNoteEvent(GetChannel(note), note.noteNumber, velocity));
        }

        void HandleNoteOff(MidiNoteControl note)
        {
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
            foreach (var device in devices)
            {
                device.onWillNoteOn -= HandleNoteOn;
                device.onWillNoteOff -= HandleNoteOff;
                device.onWillControlChange -= HandleControlChange;
            }

            devices.Clear();
            noteOn.Dispose();
            noteOff.Dispose();
            controlChange.Dispose();
        }
    }
}
