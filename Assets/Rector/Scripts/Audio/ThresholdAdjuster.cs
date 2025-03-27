using System;
using R3;
using UnityEngine;

namespace Rector.Audio
{
    // TODO: 最適化とか色々...
    public sealed class ThresholdAdjuster : IInitializable, IDisposable
    {
        readonly AudioMixerModel audioMixerModel;
        const int BufferSize = 60;
        const float AvgOffset = 0.1f;
        const float ThMin = 0.05f;
        readonly float[] lowBuffer = new float[BufferSize];
        readonly float[] midBuffer = new float[BufferSize];
        int lowIndex = 0;
        int highMidIndex = 0;

        readonly CompositeDisposable disposable = new();

        public ThresholdAdjuster(AudioMixerModel audioMixerModel)
        {
            this.audioMixerModel = audioMixerModel;
        }

        public void Initialize()
        {
            audioMixerModel.LevelLow.Subscribe(UpdateLow).AddTo(disposable);
            audioMixerModel.LevelMid.Subscribe(UpdateMid).AddTo(disposable);
        }

        void UpdateLow(float level)
        {
            lowBuffer[lowIndex] = level;
            lowIndex = (lowIndex + 1) % BufferSize;

            float max = 0;
            float avg = 0;
            for (var i = 0; i < BufferSize; i++)
            {
                var v = lowBuffer[i];
                max = Mathf.Max(max, v);
                avg += v;
            }

            avg /= BufferSize;
            audioMixerModel.ThLow.Value = Mathf.Max(avg + AvgOffset, (avg + max) * 0.5f, ThMin);
        }

        void UpdateMid(float level)
        {
            midBuffer[highMidIndex] = level;
            highMidIndex = (highMidIndex + 1) % BufferSize;

            float max = 0;
            float avg = 0;
            for (var i = 0; i < BufferSize; i++)
            {
                var v = midBuffer[i];
                max = Mathf.Max(max, v);
                avg += v;
            }

            avg /= BufferSize;
            audioMixerModel.ThMid.Value = Mathf.Max(avg + AvgOffset, (avg + max) * 0.5f, ThMin);
        }


        public void Dispose()
        {
            disposable.Dispose();
        }
    }
}
