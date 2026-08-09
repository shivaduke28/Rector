using Rector.UI.Graphs.Serialization;

namespace Rector.UI.Hud
{
    /// <summary>プリセットのスロットをHUDの文字列にする。読み込みと管理の2ページで揃える。</summary>
    public static class PresetSlotLabel
    {
        public static string Title(GraphSlotInfo info) => $"Slot {info.Number}";

        /// <summary>中身の要約。</summary>
        public static string Summary(GraphSlotInfo info) =>
            info.IsEmpty ? "(empty)" : $"{info.NodeCount} nodes / {info.EdgeCount} edges   {info.SavedAt}";

        public static string Row(GraphSlotInfo info) => $"{Title(info)}   {Summary(info)}";
    }
}
