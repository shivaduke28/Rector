using Rector.UI.Graphs.Serialization;

namespace Rector.UI.Hud
{
    /// <summary>プリセットをHUDの1行にする。読み込み・管理・確認ダイアログで同じ文面を使う。</summary>
    /// <remarks>
    /// 名前はファイル名なので長さがまちまち。等幅フォントなので、幅を決めて詰めれば
    /// 右の nodes / edges が縦に揃う。
    /// </remarks>
    public static class PresetLabel
    {
        const int NameWidth = 24;

        public static string Row(GraphPresetInfo info) =>
            $"{FitName(info.Name)}   {info.NodeCount} nodes / {info.EdgeCount} edges   {info.SavedAt}";

        static string FitName(string name) =>
            name.Length > NameWidth
                ? name[..(NameWidth - 1)] + "…"
                : name.PadRight(NameWidth);
    }
}
