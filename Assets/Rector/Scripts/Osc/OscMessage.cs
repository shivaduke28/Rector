namespace Rector.Osc
{
    public readonly struct OscMessage
    {
        public string Address { get; }

        /// <summary>第1引数を数値として読んだもの。HasValue が false のときは意味を持たない。</summary>
        public float Value { get; }

        /// <summary>数値として読める第1引数を持つか。</summary>
        /// <remarks>
        /// false になるのは、引数ゼロのメッセージ(bang)と、第1引数が文字列や blob のとき。
        /// どちらも「押された」ことしか伝えないので、Value を 0 として流すとゲージが落ち、
        /// Float 入力にも 0 が書き込まれてしまう。値の有無を分けて持ち、
        /// ノード側は Value を止めて Event だけ流す。
        /// </remarks>
        public bool HasValue { get; }

        public OscMessage(string address, float value, bool hasValue)
        {
            Address = address;
            Value = value;
            HasValue = hasValue;
        }
    }
}
