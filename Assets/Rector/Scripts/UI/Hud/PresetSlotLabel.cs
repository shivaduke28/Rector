using Rector.UI.Graphs.Serialization;

namespace Rector.UI.Hud
{
    /// <summary>プリセットのスロットをHUDの文字列にする。読み込みと管理の2ページで揃える。</summary>
    public static class PresetSlotLabel
    {
        public static string Title(GraphSlotInfo info) => $"Slot {info.Number}";

        /// <summary>中身の要約。空のスロットでは空文字。</summary>
        public static string Detail(GraphSlotInfo info) =>
            info.IsEmpty ? "" : $"{info.NodeCount} nodes / {info.EdgeCount} edges   {info.SavedAt}";

        public static string Row(GraphSlotInfo info) =>
            $"{Title(info)}   {(info.IsEmpty ? "(empty)" : Detail(info))}";
    }
}
