using System;
using R3;
using UnityEngine;

namespace Rector.Osc
{
    /// <summary>
    /// OSC 受信の設定を持つ。MidiInputDeviceManager と同じ流儀。
    ///
    /// MIDI と違って OSC には「挿さっている機器の一覧」が無いので、決められるのは
    /// 受信するかどうかと待ち受けポートだけ。ポートの候補は決め打ちで、ここに無い番号は選べない。
    /// </summary>
    public sealed class OscInputSetting : IDisposable
    {
        const string PortKey = "Rector_OscPort";
        const string EnabledKey = "Rector_OscEnabled";
        const int DefaultPort = 9000;

        public static readonly int[] PortCandidates = { 7000, 8000, 8888, 9000, 9001, 10000 };

        // 初期値は未ロード。OscModel はこの状態では何もしないので、
        // Reload を呼ぶまでソケットは開かないしログも出ない
        readonly ReactiveProperty<OscInputConfig> config = new(new OscInputConfig(false, true, DefaultPort));
        public ReadOnlyReactiveProperty<OscInputConfig> Config => config;

        /// <summary>
        /// 保存値を読んで公開する。これを呼ぶまでポートは開かない。
        /// 呼ぶタイミングは RectorInstaller が決める。
        /// </summary>
        public void Reload()
        {
            var port = PlayerPrefs.GetInt(PortKey, DefaultPort);
            if (Array.IndexOf(PortCandidates, port) < 0) port = DefaultPort;

            var enabled = PlayerPrefs.GetInt(EnabledKey, 1) != 0;
            config.Value = new OscInputConfig(true, enabled, port);
        }

        public void SetEnabled(bool enabled)
        {
            var current = config.Value;
            if (current.Enabled == enabled) return;

            config.Value = new OscInputConfig(current.Loaded, enabled, current.Port);
            PlayerPrefs.SetInt(EnabledKey, enabled ? 1 : 0);
        }

        public void SetPort(int port)
        {
            if (Array.IndexOf(PortCandidates, port) < 0) return;

            var current = config.Value;
            if (current.Port == port) return;

            config.Value = new OscInputConfig(current.Loaded, current.Enabled, port);
            PlayerPrefs.SetInt(PortKey, port);
        }

        public void Dispose() => config.Dispose();
    }
}
