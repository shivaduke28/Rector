using R3;

namespace Rector.UI.Graphs.Slots
{
    public interface ISlot
    {
        NodeId NodeId { get; }
        SlotDirection Direction { get; }
        string Name { get; }
        ReactiveProperty<bool> Selected { get; }

        /// <summary>エッジ作成のターゲットとして指されている。ソース(Selected)とは別の見た目になる。</summary>
        ReactiveProperty<bool> IsTarget { get; }
        SlotValueType Type { get; }
        int Index { get; }
        int ConnectedCount { get; }
        void OnConnected();
        void Disconnected();
    }
}
