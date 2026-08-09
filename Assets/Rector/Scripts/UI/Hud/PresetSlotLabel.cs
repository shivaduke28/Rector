using Rector.UI.Graphs.Serialization;

namespace Rector.UI.Hud
{
    /// <summary>プリセットのスロットをHUDの1行にする。読み込み・管理・確認ダイアログで同じ文面を使う。</summary>
    public static class PresetSlotLabel
    {
        public static string Row(GraphSlotInfo info)
        {
            var summary = info.IsEmpty
                ? "(empty)"
                : $"{info.NodeCount} nodes / {info.EdgeCount} edges   {info.SavedAt}";

            return $"Slot {info.Number}   {summary}";
        }
    }
}
