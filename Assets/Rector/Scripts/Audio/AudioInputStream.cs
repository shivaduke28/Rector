using System;
using Lasp;
using Unity.Collections;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Rector.Audio
{
    /// <summary>
    /// Laspのwrapper
    /// </summary>
    public sealed class AudioInputStream : IDisposable
    {
        readonly AudioLevelTracker levelTracker;
        readonly AudioLevelTracker lowTracker;
        readonly AudioLevelTracker midTracker;
        readonly AudioLevelTracker highTracker;
        readonly SpectrumAnalyzer spectrum;

        public float Level => levelTracker.normalizedLevel;
        public float LevelLow => lowTracker.normalizedLevel;
        public float LevelMid => midTracker.normalizedLevel;
        public float LevelHigh => highTracker.normalizedLevel;

        /// <summary>
        /// 音声データ（波形）。マイフレームサイズが変わるので注意。
        /// </summary>
        public NativeSlice<float> AudioDataSlice => levelTracker.audioDataSlice;

        /// <summary>
        /// スペクトルをログスケールしたもの。サイズは<see cref="SpectrumSize"/>で固定。
        /// 呼ぶたびにログスケールが実行されるので注意。
        /// </summary>
        public NativeArray<float> LogSpectrum => spectrum.logSpectrumArray;

        public const int SpectrumSize = 512;


        AudioInputStream(AudioLevelTracker levelTracker,
            AudioLevelTracker lowTracker,
            AudioLevelTracker midTracker,
            AudioLevelTracker highTracker,
            SpectrumAnalyzer spectrum)
        {
            this.levelTracker = levelTracker;
            this.lowTracker = lowTracker;
            this.midTracker = midTracker;
            this.highTracker = highTracker;
            this.spectrum = spectrum;
        }

        public static AudioInputStream Create(AudioInputDeviceInfo info, int channel, Transform parent)
        {
            var go = new GameObject(info.Name);
            go.transform.SetParent(parent, false);

            var levelTracker = go.AddComponent<AudioLevelTracker>();
            levelTracker.deviceID = info.Id;
            levelTracker.channel = channel;
            levelTracker.filterType = FilterType.Bypass;

            var lowTracker = go.AddComponent<AudioLevelTracker>();
            lowTracker.deviceID = info.Id;
            lowTracker.channel = channel;
            lowTracker.filterType = FilterType.LowPass;

            var midTracker = go.AddComponent<AudioLevelTracker>();
            midTracker.deviceID = info.Id;
            midTracker.channel = channel;
            midTracker.filterType = FilterType.BandPass;

            var highTracker = go.AddComponent<AudioLevelTracker>();
            highTracker.deviceID = info.Id;
            highTracker.channel = channel;
            highTracker.filterType = FilterType.HighPass;

            var spectrum = go.AddComponent<SpectrumAnalyzer>();
            spectrum.deviceID = info.Id;

            return new AudioInputStream(
                levelTracker,
                lowTracker,
                midTracker,
                highTracker,
                spectrum);
        }

        public void Dispose()
        {
            Object.Destroy(levelTracker.gameObject);
        }
    }
}
