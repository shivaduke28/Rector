using R3;

namespace Rector.UI.Graphs.Slots
{
    /// <summary>
    /// 値を保持する int 入力スロット。HUD のパラメータ行と保存/復元はこの窓口だけを見る。
    /// 裏が ReactiveProperty（同値を捨てる）か BehaviorSubject（毎回流す）かはノード側の都合。
    /// </summary>
    public interface IIntValueInputSlot : ISlot
    {
        /// <summary>
        /// 現在値。set は HUD の編集とプリセット復元のための入口で、ミュート中でも書ける。
        /// ミュートが止めるのはワイヤ（Send）と出力だけで、パラメータ操作は別（ReactivePropertyInputSlot.Property と同じ扱い）。
        /// </summary>
        int Value { get; set; }
        int MinValue { get; }
        int MaxValue { get; }
        Observable<int> Observable();
    }
}
