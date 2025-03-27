using System;
using Lasp;
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

        // db
        public ReactiveProperty<float> Gain { get; } = new(12f);

        // normalize levels
        public ReadOnlyReactiveProperty<float> Level => level;
        public ReadOnlyReactiveProperty<float> LevelLow => levelLow;
        public ReadOnlyReactiveProperty<float> LevelMid => levelMid;
        public ReadOnlyReactiveProperty<float> LevelHigh => levelHigh;

        public ReactiveProperty<float> EqLow { get; } = new(1f);

        public ReactiveProperty<float> EqMid { get; } = new(1f);

        public ReactiveProperty<float> EqHigh { get; } = new(1f);

        public ReactiveProperty<float> ThLow { get; } = new(1f);


        public ReactiveProperty<float> ThMid { get; } = new(1f);

        public ReactiveProperty<float> ThHigh { get; } = new(1f);

        IDisposable disposable;

        readonly LevelNormalizer lowNormalizer = new();
        readonly LevelNormalizer midNormalizer = new();
        readonly LevelNormalizer highNormalizer = new();
        readonly LevelNormalizer levelNormalizer = new();
        readonly LevelNormalizer fftNormalizer = new();

        const int Resolution = 512;
        readonly FftBuffer fftBuffer = new(Resolution * 2);
        readonly LogScaler logScaler = new();

        static readonly int AudioSpectrumPropertyId = Shader.PropertyToID("_AudioSpectrum");
        static readonly int WaveformPropertyId = Shader.PropertyToID("_Waveform");
        static readonly int WaveformSizePropertyId = Shader.PropertyToID("_WaveformSize");
        readonly float[] waveform = new float[Resolution];

        GraphicsBuffer spectrumBuffer;
        GraphicsBuffer waveformBuffer;


        public AudioMixerModel(AudioInputDeviceManager audioInputDeviceManager)
        {
            this.audioInputDeviceManager = audioInputDeviceManager;

            fftNormalizer.Silence = -240f;
            fftNormalizer.DynamicRange = 80;
        }

        public void Initialize()
        {
            disposable = Observable.EveryUpdate(UnityFrameProvider.Update)
                .Subscribe(_ => Update());

            spectrumBuffer = new GraphicsBuffer(GraphicsBuffer.Target.Structured, Resolution, sizeof(float));
            Shader.SetGlobalBuffer(AudioSpectrumPropertyId, spectrumBuffer);

            waveformBuffer = new GraphicsBuffer(GraphicsBuffer.Target.Structured, Resolution, sizeof(float));
            Shader.SetGlobalBuffer(WaveformPropertyId, waveformBuffer);
        }

        void Update()
        {
            var dt = Time.deltaTime;
            var input = audioInputDeviceManager.GetInputLevels();
            level.Value = levelNormalizer.Input(input[0], dt);
            levelLow.Value = lowNormalizer.Input(input[1], dt);
            levelMid.Value = midNormalizer.Input(input[2], dt);
            levelHigh.Value = highNormalizer.Input(input[3], dt);
            fftNormalizer.Input(input[0], dt);
            var slice = audioInputDeviceManager.GetChannelDataSlice();

            var len = Mathf.Min(slice.Length, Resolution);
            for (var i = 0; i < len; i++)
            {
                waveform[i] = slice[i];
            }

            waveformBuffer.SetData(waveform);
            Shader.SetGlobalInt(WaveformSizePropertyId, len);

            fftBuffer.Push(slice);
            fftBuffer.Analyze(-fftNormalizer.CurrentGain - fftNormalizer.DynamicRange, -fftNormalizer.CurrentGain);

            spectrumBuffer.SetData(logScaler.Resample(fftBuffer.Spectrum));
        }

        public void Dispose()
        {
            fftBuffer?.Dispose();
            logScaler?.Dispose();
            disposable?.Dispose();
            spectrumBuffer?.Dispose();
            waveformBuffer?.Dispose();
        }


        sealed class LevelNormalizer
        {
            public bool AutoGain = true;
            public bool SmoothFall = true;
            float gain;
            float head;
            float fall;
            const float DecaySpeed = 0.6f;
            public float DynamicRange = 12f;
            const float FallSpeed = 0.3f;
            public float Silence = -60f; // dBFS
            public float CurrentGain => AutoGain ? -head : gain;
            float normalizedLevel;

            public float Input(float input, float dt)
            {
                // Auto gain control
                if (AutoGain)
                {
                    // Slowly return to the noise floor.
                    head = Mathf.Max(head - DecaySpeed * dt, Silence);

                    // Pull up by input with a small headroom.
                    var room = DynamicRange * 0.05f;
                    head = Mathf.Clamp(input - room, head, 0);
                }

                // Normalize the input value.
                var normalizedInput
                    = Mathf.Clamp01((input + CurrentGain) / DynamicRange + 1);

                if (SmoothFall)
                {
                    // Hold and fall down animation
                    fall += Mathf.Pow(10, 1 + FallSpeed * 2) * dt;
                    normalizedLevel -= fall * dt;

                    // Pull up by input.
                    if (normalizedLevel < normalizedInput)
                    {
                        normalizedLevel = normalizedInput;
                        fall = 0;
                    }
                }
                else
                {
                    normalizedLevel = normalizedInput;
                }

                return normalizedLevel;
            }
        }
    }
}
