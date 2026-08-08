using System;
using System.Collections.Generic;
using System.Linq;
using R3;
using UnityEngine;
using RtMidiIn = RtMidi.MidiIn;

namespace Rector.Midi
{
    /// <summary>
    /// 受信対象の MIDI ポートの選択を持つ。AudioInputDeviceManager と同じ流儀。
    ///
    /// 一覧は Minis 経由ではなく RtMidi のプローブから取る。Minis の MidiDevice は
    /// 最初のメッセージを受信するまで生えてこないので、そちらを一覧にすると
    /// 「挿したのにリストが空」になってしまう。プローブは OpenPort しないので
    /// 受信そのものには一切関与しない。
    ///
    /// NOTE: Minis の MidiDriver は常に全ポートを開くため、選択から外したポートも
    /// Rector が握ったままになる。ここでできるのはイベントの取捨選択だけ。
    /// </summary>
    public sealed class MidiInputDeviceManager : IDisposable
    {
        const string PrefsKey = "Rector_MidiInputDevices";
        const string Separator = "\n";

        readonly HashSet<string> selected = new();
        readonly ReactiveProperty<string[]> selectedDevices = new(Array.Empty<string>());

        // R3 の既定比較子は EqualityComparer<T>.Default で、配列は Equals を上書きしないので
        // 参照等価になる。中身を書き換えず必ず新しい配列を代入すること
        public ReadOnlyReactiveProperty<string[]> SelectedDevices => selectedDevices;

        RtMidiIn probe;
        bool probeFailed;

        public string[] GetInputDevices()
        {
            var p = GetProbe();
            if (p == null) return Array.Empty<string>();

            try
            {
                // PortCount はフィールドではなく毎回 P/Invoke なのでスナップショットする
                var count = p.PortCount;
                var names = new string[count];
                for (var i = 0; i < count; i++)
                {
                    names[i] = p.GetPortName(i);
                }

                return names;
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to enumerate MIDI input ports: {e.Message}");
                return Array.Empty<string>();
            }
        }

        public void Toggle(string portName)
        {
            if (string.IsNullOrEmpty(portName)) return;

            SetSelected(portName, !selected.Contains(portName));
        }

        public void SetSelected(string portName, bool isSelected)
        {
            if (string.IsNullOrEmpty(portName)) return;

            var changed = isSelected ? selected.Add(portName) : selected.Remove(portName);
            if (!changed) return;

            Publish();
            PlayerPrefs.SetString(PrefsKey, string.Join(Separator, selected));
            RectorLogger.MidiInputDeviceSelection(portName, isSelected);
        }

        public void ReloadSelection()
        {
            selected.Clear();

            // RemoveEmptyEntries が無いと "" が [""] になり、product が空のデバイスが
            // 選択済み扱いで通ってしまう
            var stored = PlayerPrefs.GetString(PrefsKey, "");
            foreach (var portName in stored.Split(new[] { Separator }, StringSplitOptions.RemoveEmptyEntries))
            {
                selected.Add(portName);
            }

            Publish();
        }

        void Publish() => selectedDevices.Value = selected.ToArray();

        RtMidiIn GetProbe()
        {
            if (probe != null || probeFailed) return probe;

            try
            {
                var created = RtMidiIn.Create();
                // IsOk はハンドルの指す先を読むので、先に IsInvalid (マネージド側の判定) で弾く
                if (created == null || created.IsInvalid || !created.IsOk)
                {
                    created?.Dispose();
                    probeFailed = true;
                    return null;
                }

                probe = created;
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to create a MIDI probe: {e.Message}");
                probeFailed = true;
            }

            return probe;
        }

        public void Dispose()
        {
            probe?.Dispose();
            probe = null;
            selectedDevices.Dispose();
        }
    }
}
