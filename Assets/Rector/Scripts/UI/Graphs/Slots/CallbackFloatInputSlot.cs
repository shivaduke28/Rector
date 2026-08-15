using System;
using R3;

namespace Rector.UI.Graphs.Slots
{
    public sealed class CallbackFloatInputSlot : InputSlot<float>, ICallbackInputSlot
    {
        readonly Subject<float> subject = new();
        readonly Action<float> action;
        readonly ReadOnlyReactiveProperty<bool> isMuted;

        public CallbackFloatInputSlot(NodeId nodeId, int index, string name, Action<float> action, ReadOnlyReactiveProperty<bool> isMuted) : base(nodeId, index, name)
        {
            this.action = action;
            this.isMuted = isMuted;
        }

        public void SendForce()
        {
            action.Invoke(1f);
            subject.OnNext(1f);
        }

        public override void Send(float value)
        {
            if (isMuted.CurrentValue) return;
            action.Invoke(value);
            subject.OnNext(value);
        }

        public override Observable<float> Observable() => subject;
    }
}
