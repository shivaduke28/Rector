using System;
using R3;

namespace Rector.UI.Graphs.Slots
{
    public sealed class CallbackFloatInputSlot : InputSlot<float>
    {
        readonly Subject<float> subject = new();
        readonly Action<float> action;
        readonly ReadOnlyReactiveProperty<bool> isMuted;
        // 表示専用。ワイヤには乗せない（リプレイを生まない）
        readonly ReactiveProperty<float> latestValue = new(0f);

        public float MinValue { get; }
        public float MaxValue { get; }
        public ReadOnlyReactiveProperty<float> LatestValue => latestValue;

        public CallbackFloatInputSlot(NodeId nodeId, int index, string name, Action<float> action, float minValue, float maxValue, ReadOnlyReactiveProperty<bool> isMuted) : base(nodeId, index, name)
        {
            this.action = action;
            MinValue = minValue;
            MaxValue = maxValue;
            this.isMuted = isMuted;
        }

        public override void Send(float value)
        {
            if (isMuted.CurrentValue) return;
            latestValue.Value = value;
            action.Invoke(value);
            subject.OnNext(value);
        }

        public override Observable<float> Observable() => subject;
    }
}
