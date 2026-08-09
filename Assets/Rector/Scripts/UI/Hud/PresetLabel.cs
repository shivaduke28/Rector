using Rector.UI.Graphs.Serialization;

namespace Rector.UI.Hud
{
    /// <summary>プリセットをHUDの1行にする。読み込み・管理・確認ダイアログで同じ文面を使う。</summary>
    /// <remarks>
    /// 名前はファイル名なので長さがまちまちになる。列を揃えないと右の nodes / edges が
    /// 行ごとに横へずれる。等幅フォントなので、幅を決めて詰めれば揃う。
    /// </remarks>
    public static class PresetLabel
    {
        const int NameWidth = 24;

        public static string Row(GraphPresetInfo info) =>
            $"{FitName(info.Name)}   {info.NodeCount} nodes / {info.EdgeCount} edges   {info.SavedAt}";

        /// <summary>名前を名前列の幅に合わせる。長ければ切り、短ければ詰める。</summary>
        static string FitName(string name) =>
            name.Length > NameWidth
                ? name[..(NameWidth - 1)] + "…"
                : name.PadRight(NameWidth);
    }
}
