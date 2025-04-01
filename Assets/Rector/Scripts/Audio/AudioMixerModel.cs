using System;
using R3;
using UnityEngine;

namespace Rector.Audio
{
    public sealed class AudioMixerModel : IInitializable, IDisposable
    {
        readonly AudioInputDeviceManager audioInputDeviceManager;

        readonly ReactiveProperty<float> level = new(0f);
        readonly ReactiveProperty<float> levelLow = new(0f);
        readonly ReactiveProperty<float> levelMid = new(0f);
        readonly ReactiveProperty<float> levelHigh = new(0f);

        // normalize levels
        public ReadOnlyReactiveProperty<float> Level => level;
        public ReadOnlyReactiveProperty<float> LevelLow => levelLow;
        public ReadOnlyReactiveProperty<float> LevelMid => levelMid;
        public ReadOnlyReactiveProperty<float> LevelHigh => levelHigh;

        public ReactiveProperty<float> ThLow { get; } = new(1f);
        public ReactiveProperty<float> ThMid { get; } = new(1f);
        public ReactiveProperty<float> ThHigh { get; } = new(1f);

        IDisposable disposable;

        static readonly int AudioSpectrumPropertyId = Shader.PropertyToID("_RectorAudioSpectrum");
        static readonly int WaveformPropertyId = Shader.PropertyToID("_RectorAudioWaveform");
        static readonly int WaveformSizePropertyId = Shader.PropertyToID("_RectorAudioWaveformSize");
        readonly float[] waveform = new float[AudioInputStream.SpectrumSize];

        GraphicsBuffer spectrumBuffer;
        GraphicsBuffer waveformBuffer;


        public AudioMixerModel(AudioInputDeviceManager audioInputDeviceManager)
        {
            this.audioInputDeviceManager = audioInputDeviceManager;
        }

        public void Initialize()
        {
            disposable = Observable.EveryUpdate(UnityFrameProvider.Update)
                .Subscribe(_ => Update());

            spectrumBuffer = new GraphicsBuffer(GraphicsBuffer.Target.Structured, AudioInputStream.SpectrumSize, sizeof(float));
            Shader.SetGlobalBuffer(AudioSpectrumPropertyId, spectrumBuffer);

            waveformBuffer = new GraphicsBuffer(GraphicsBuffer.Target.Structured, AudioInputStream.SpectrumSize, sizeof(float));
            Shader.SetGlobalBuffer(WaveformPropertyId, waveformBuffer);
        }

        void Update()
        {
            var stream = audioInputDeviceManager.InputStream;
            if (stream is not { IsValid: true } ) return;

            try
            {
                level.Value = stream.Level;
                levelLow.Value = stream.LevelLow;
                levelMid.Value = stream.LevelMid;
                levelHigh.Value = stream.LevelHigh;

                // NOTE: sliceの長さが可変なのでminを取ってshaderにも配る
                var audioDataLice = stream.AudioDataSlice;
                var len = Mathf.Min(audioDataLice.Length, AudioInputStream.SpectrumSize);
                for (var i = 0; i < len; i++)
                {
                    waveform[i] = audioDataLice[i];
                }

                waveformBuffer.SetData(waveform);
                Shader.SetGlobalInt(WaveformSizePropertyId, len);

                spectrumBuffer.SetData(stream.LogSpectrum);
            }
            catch (InvalidOperationException e)
            {
                Debug.LogException(e);
                audioInputDeviceManager.Clear();
            }
        }

        public void Dispose()
        {
            disposable?.Dispose();
            spectrumBuffer?.Dispose();
            waveformBuffer?.Dispose();
        }
    }
}
