#nullable enable
using R3;

namespace Rector.UI.Graphs.Slots
{
    // abstractにしてValueTypeごとに作った方がいいかもしれん

    public class ReactivePropertyInputSlot<T> : InputSlot<T>
    {
        // Muteを見ていないのでグラフからは使わないようにする
        public readonly ReactiveProperty<T> Property;
        readonly T? defaultValue;
        readonly ReadOnlyReactiveProperty<bool> isMuted;

        public override void Send(T value)
        {
            if (isMuted.CurrentValue) return;
            Property.Value = value;
        }

        public override Observable<T> Observable() => Property;

        public override void Disconnected()
        {
            base.Disconnected();
            if (ConnectedCount == 0 && defaultValue != null)
            {
                Property.Value = defaultValue;
            }
        }

        public ReactivePropertyInputSlot(NodeId nodeId, int index, string name, ReactiveProperty<T> property, T defaultValue, ReadOnlyReactiveProperty<bool> isMuted) : base(
            nodeId, index, name)
        {
            Property = property;
            this.defaultValue = defaultValue;
            this.isMuted = isMuted;
        }
    }
}
