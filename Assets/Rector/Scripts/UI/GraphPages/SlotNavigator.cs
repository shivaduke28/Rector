using Rector.UI.Graphs.Slots;

#nullable enable

namespace Rector.UI.GraphPages
{
    /// <summary>スロット選択のカーソル移動先を決める。ビューに触らない純粋な計算のみ。</summary>
    public static class SlotNavigator
    {
        /// <summary>
        /// 指定した向きのスロットの先頭を返す。その向きが空なら反対側の先頭に落とし、
        /// どちらも空なら null。
        /// </summary>
        public static ISlot? PickInDirection(SlotDirection direction, InputSlot[] inputSlots, OutputSlot[] outputSlots)
        {
            if (direction == SlotDirection.Output)
            {
                if (outputSlots.Length > 0) return outputSlots[0];
                return inputSlots.Length > 0 ? inputSlots[0] : null;
            }

            if (inputSlots.Length > 0) return inputSlots[0];
            return outputSlots.Length > 0 ? outputSlots[0] : null;
        }
    }
}
