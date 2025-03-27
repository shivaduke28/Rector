using System;
using R3;
using Unity.Profiling;

namespace Rector
{
    public sealed class MemoryStatsRecorder : IInitializable, IDisposable
    {
        ProfilerRecorder systemUsedMemoryRecorder;
        ProfilerRecorder totalUsedMemoryRecorder;

        public readonly ReactiveProperty<float> SystemUsedMemory = new();
        public readonly ReactiveProperty<float> TotalUsedMemory = new();

        IDisposable disposable;

        public void Initialize()
        {
            systemUsedMemoryRecorder = ProfilerRecorder.StartNew(ProfilerCategory.Memory, "System Used Memory");
            totalUsedMemoryRecorder = ProfilerRecorder.StartNew(ProfilerCategory.Memory, "Total Used Memory");

            disposable = Observable.Interval(TimeSpan.FromSeconds(1))
                .Subscribe(_ =>
                {
                    SystemUsedMemory.Value = systemUsedMemoryRecorder.LastValue;
                    TotalUsedMemory.Value = totalUsedMemoryRecorder.LastValue;
                });
        }

        public void Dispose()
        {
            systemUsedMemoryRecorder.Dispose();
            totalUsedMemoryRecorder.Dispose();
            disposable?.Dispose();
        }
    }
}
