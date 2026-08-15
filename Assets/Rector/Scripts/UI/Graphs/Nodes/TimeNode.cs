using System;
using R3;
using Rector.NodeBehaviours;
using Rector.UI.Graphs.Slots;
using UnityEngine;

namespace Rector.UI.Graphs.Nodes
{
    public sealed class TimeNode : SourceNode, IDisposable
    {
        public const string NodeName = "Time";
        public static NodeCategory GetCategory() => NodeCategory.Event;
        public override NodeCategory Category => GetCategory();

        readonly FloatInput scaleInput = new("scale", 1f, float.NegativeInfinity, float.PositiveInfinity);

        // 蓄積はノード所有の購読1本で行う。出力パイプラインの中で蓄積すると
        // エッジごとに独立購読されて、エッジの本数だけ時間が速く進んでしまう
        readonly ReactiveProperty<float> scaledTime = new(0f);
        readonly IDisposable subscription;

        public TimeNode(NodeId id) : base(id, NodeName)
        {
            InputSlots = new[]
            {
                SlotConverter.Convert(id, 0, ActiveInput, IsMuted),
                SlotConverter.Convert(id, 1, scaleInput, IsMuted),
            };

            subscription = Observable.EveryUpdate(UnityFrameProvider.Update)
                .Where(_ => IsActive)
                .Subscribe(_ => scaledTime.Value += Time.deltaTime * scaleInput.Value.Value);

            OutputSlots = new OutputSlot[]
            {
                new ObservableOutputSlot<float>(id, 0, "time", scaledTime, IsMuted),
                new ObservableOutputSlot<float>(id, 1, "frac", scaledTime.Select(t => t % 1), IsMuted)
            };
        }

        public override InputSlot[] InputSlots { get; }
        public override OutputSlot[] OutputSlots { get; }

        public void Dispose()
        {
            subscription.Dispose();
            scaledTime.Dispose();
        }
    }
}
