namespace Rector.UI.Settings
{
    /// <summary>
    /// 設定行の共通USSクラス名。ステッパーとセレクターでラベル列と値列の骨格を揃え、
    /// 行の種類が変わっても値の位置が動かないようにする。
    /// </summary>
    public static class SettingRowUss
    {
        public const string Row = "rector-setting-row";
        public const string RowFocused = Row + "--focused";
        public const string Label = Row + "__label";
        public const string Value = Row + "__value";
        public const string ValueLabel = Row + "__value-label";
    }
}
