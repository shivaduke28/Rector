using R3;

namespace Rector.UI.Graphs.Slots
{
    public sealed class ObservableOutputSlot<T> : OutputSlot<T>
    {
        readonly Observable<T> observable;
        readonly ReadOnlyReactiveProperty<bool> isMuted;

        public override Observable<T> Observable() => observable.Where(_ => !isMuted.CurrentValue);

        public ObservableOutputSlot(NodeId nodeId, int index, string name, Observable<T> observable, ReadOnlyReactiveProperty<bool> isMuted) : base(nodeId,
            index, name)
        {
            this.observable = observable;
            this.isMuted = isMuted;
        }
    }
}
