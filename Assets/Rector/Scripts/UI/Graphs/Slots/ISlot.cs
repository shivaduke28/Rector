using R3;

namespace Rector.UI.Graphs.Slots
{
    public interface ISlot
    {
        NodeId NodeId { get; }
        SlotDirection Direction { get; }
        string Name { get; }
        ReactiveProperty<bool> Selected { get; }
        SlotValueType Type { get; }
        int Index { get; }
        int ConnectedCount { get; }
        void OnConnected();
        void Disconnected();
    }
}
