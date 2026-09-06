namespace Rector.UI.Graphs.Slots
{
    /// <summary>値を保持する int 入力スロット。HUD のスライダーが使う範囲を持つ。</summary>
    public interface IIntValueInputSlot : IValueInputSlot<int>
    {
        int MinValue { get; }
        int MaxValue { get; }
    }
}
