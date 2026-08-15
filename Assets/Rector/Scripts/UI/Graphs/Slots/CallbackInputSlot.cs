using System;
using R3;

namespace Rector.UI.Graphs.Slots
{
    public sealed class CallbackInputSlot : InputSlot<Unit>
    {
        readonly Subject<Unit> subject = new();
        readonly Action action;
        readonly ReadOnlyReactiveProperty<bool> isMuted;

        public CallbackInputSlot(NodeId nodeId, int index, string name, Action action, ReadOnlyReactiveProperty<bool> isMuted) : base(nodeId, index, name)
        {
            this.action = action;
            this.isMuted = isMuted;
        }

        public void SendForce()
        {
            action.Invoke();
            subject.OnNext(Unit.Default);
        }

        public override void Send(Unit value)
        {
            if (isMuted.CurrentValue) return;
            action.Invoke();
            subject.OnNext(value);
        }

        public override Observable<Unit> Observable() => subject;
    }
}
