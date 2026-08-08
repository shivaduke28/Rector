using System;

namespace Rector.Midi
{
    /// <summary>
    /// Minis の MidiDevice 名から MIDI ポート名を取り出す。
    /// Minis は (ポート x チャンネル) ごとに MidiDevice を生やし、その product を
    /// "&lt;ポート名&gt; Channel &lt;n&gt;" として組み立てる (Minis.MidiUtility.MakeDeviceDescription)。
    /// Rector はポート単位で受信可否を決めるので、その規約をここで剥がす。
    /// </summary>
    public static class MidiPortName
    {
        const string ChannelSuffix = " Channel ";

        public static string FromProduct(string product)
        {
            if (string.IsNullOrEmpty(product)) return "";

            // ポート名自体が " Channel " を含みうるので、末尾側の区切りを採用する
            var index = product.LastIndexOf(ChannelSuffix, StringComparison.Ordinal);
            return index < 0 ? product : product.Substring(0, index);
        }
    }
}
