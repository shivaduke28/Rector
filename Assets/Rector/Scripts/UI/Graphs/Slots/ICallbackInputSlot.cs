namespace Rector.UI.Graphs.Slots
{
    /// <summary>
    /// パラメータパネルの fire ボタンから叩ける、イベント型（Subject起点・dedupなし）の入力スロット。
    /// </summary>
    public interface ICallbackInputSlot
    {
        string Name { get; }
        void SendForce();
    }
}
