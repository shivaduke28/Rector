using R3;

namespace Rector.UI.Graphs.Slots
{
    public abstract class InputSlot<T> : InputSlot
    {
        public abstract void Send(T value);

        public abstract Observable<T> Observable();

        protected InputSlot(NodeId nodeId, int index, string name) : base(nodeId, index,
            SlotUtils.GetSlotValueType<T>(), name)
        {
        }
    }

    public abstract class InputSlot : ISlot
    {
        public NodeId NodeId { get; }
        public int Index { get; }
        public SlotValueType Type { get; }
        public SlotDirection Direction => SlotDirection.Input;
        public string Name { get; }
        public ReactiveProperty<bool> Selected { get; } = new(false);
        public int ConnectedCount { get; private set; }

        public virtual void OnConnected()
        {
            ConnectedCount++;
        }

        public virtual void Disconnected()
        {
            ConnectedCount--;
        }

        protected InputSlot(NodeId nodeId, int index, SlotValueType type, string name)
        {
            NodeId = nodeId;
            Index = index;
            Name = name;
            Type = type;
        }
    }
}
