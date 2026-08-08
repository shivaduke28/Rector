using System;

namespace Rector.Osc
{
    /// <summary>
    /// OSC 受信の設定。有効/無効とポートは必ず一組で扱う。
    /// </summary>
    /// <remarks>
    /// 別々の ReactiveProperty にすると、起動時に両方を入れる場面で購読が二度走り、
    /// 使わないポートを一度 bind してから張り直すことになる。まとめて1回だけ流す。
    /// </remarks>
    public readonly struct OscInputConfig : IEquatable<OscInputConfig>
    {
        /// <summary>保存値をまだ読んでいない。この状態では bind もログもしない。</summary>
        public bool Loaded { get; }

        public bool Enabled { get; }
        public int Port { get; }

        public OscInputConfig(bool loaded, bool enabled, int port)
        {
            Loaded = loaded;
            Enabled = enabled;
            Port = port;
        }

        public bool Equals(OscInputConfig other)
            => Loaded == other.Loaded && Enabled == other.Enabled && Port == other.Port;

        public override bool Equals(object obj) => obj is OscInputConfig other && Equals(other);

        public override int GetHashCode() => HashCode.Combine(Loaded, Enabled, Port);
    }
}
