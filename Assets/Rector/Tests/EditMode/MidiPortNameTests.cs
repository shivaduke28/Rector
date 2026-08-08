using NUnit.Framework;
using Rector.Midi;

namespace Rector.Tests.EditMode
{
    /// <summary>
    /// MidiPortName.FromProduct のテスト。
    /// 入力は Minis.MidiUtility.MakeDeviceDescription が組み立てる product 文字列
    /// ("&lt;ポート名&gt; Channel &lt;n&gt;")。
    /// </summary>
    public sealed class MidiPortNameTests
    {
        [Test]
        public void FromProduct_StripsChannelSuffix()
        {
            Assert.AreEqual("nanoKONTROL2 SLIDER/KNOB",
                MidiPortName.FromProduct("nanoKONTROL2 SLIDER/KNOB Channel 0"));
            Assert.AreEqual("Launchkey Mini MK3 MIDI",
                MidiPortName.FromProduct("Launchkey Mini MK3 MIDI Channel 15"));
        }

        [Test]
        public void FromProduct_UsesTheLastSeparator()
        {
            // ポート名自体が " Channel " を含むケース
            Assert.AreEqual("My Channel Strip", MidiPortName.FromProduct("My Channel Strip Channel 3"));
        }

        [Test]
        public void FromProduct_ReturnsInputWhenSuffixIsAbsent()
        {
            Assert.AreEqual("NoSuffix", MidiPortName.FromProduct("NoSuffix"));
        }

        [Test]
        public void FromProduct_ReturnsEmptyForNullOrEmpty()
        {
            Assert.AreEqual("", MidiPortName.FromProduct(null));
            Assert.AreEqual("", MidiPortName.FromProduct(""));
        }
    }
}
