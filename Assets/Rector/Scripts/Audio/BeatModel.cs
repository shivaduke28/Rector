using System;
using System.Collections.Generic;
using System.Linq;
using R3;
using UnityEngine;

namespace Rector.Audio
{
    public sealed class BeatModel : IInitializable, IDisposable
    {
        readonly ReactiveProperty<float> bpmProperty = new(120f);
        readonly ReactiveProperty<int> beatProperty = new(0);
        readonly CompositeDisposable disposables = new();

        readonly SerialDisposable beatDisposable = new();

        const int TapTempoCapacity = 8;
        readonly List<float> tapTempIntervals = new(TapTempoCapacity);
        int tapCount;

        // とりあえず0~3
        public ReadOnlyReactiveProperty<int> BeatProperty => beatProperty;
        public ReadOnlyReactiveProperty<float> BpmProperty => bpmProperty;

        float lastTapTime = -100;

        public void Initialize()
        {
            bpmProperty.Subscribe(bpm =>
            {
                beatProperty.Value = 3;
                beatDisposable.Disposable = Observable.Interval(TimeSpan.FromSeconds(60f / bpm))
                    .Subscribe(_ => beatProperty.Value = (beatProperty.Value + 1) % 4);
            }).AddTo(disposables);

            beatDisposable.AddTo(disposables);
        }

        public void Tap()
        {
            var now = Time.realtimeSinceStartup;
            var elapsed = now - lastTapTime;
            lastTapTime = now;

            if (elapsed > 1f)
            {
                tapCount = 0;
                tapTempIntervals.Clear();
                return;
            }
            var tempo = 60f / elapsed;
            if (tapCount < TapTempoCapacity)
            {
                tapTempIntervals.Add(tempo);
            }
            else
            {
                tapTempIntervals[tapCount % TapTempoCapacity] = tempo;
            }
            tapCount++;
            bpmProperty.Value = tapTempIntervals.Sum() / tapTempIntervals.Count;
        }

        public void Dispose()
        {
            disposables.Dispose();
        }
    }
}
