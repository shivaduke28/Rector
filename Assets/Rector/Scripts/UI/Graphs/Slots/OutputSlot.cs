using R3;

namespace Rector.UI.Graphs.Slots
{
    public abstract class OutputSlot : ISlot
    {
        public NodeId NodeId { get; }
        public int Index { get; }
        public int ConnectedCount { get; private set; }
        public void OnConnected() => ConnectedCount++;

        public void Disconnected() => ConnectedCount--;

        public SlotValueType Type { get; }
        public SlotDirection Direction => SlotDirection.Output;
        public string Name { get; }
        public ReactiveProperty<bool> Selected { get; } = new(false);

        protected OutputSlot(NodeId nodeId, int index, SlotValueType type, string name)
        {
            NodeId = nodeId;
            Index = index;
            Name = name;
            Type = type;
        }
    }

    public abstract class OutputSlot<T> : OutputSlot
    {
        public abstract Observable<T> Observable();

        protected OutputSlot(NodeId nodeId, int index, string name) : base(nodeId, index,
            SlotUtils.GetSlotValueType<T>(), name)
        {
        }
    }
}
