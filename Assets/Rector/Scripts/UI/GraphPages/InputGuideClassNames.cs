namespace Rector.UI.GraphPages
{
    /// <summary>
    /// 操作ガイドのUSSクラス名。パッド用とキーボード用のレイアウトでチップの見た目
    /// (減光・反転)を共有するので、名前は一箇所にまとめておく。
    /// </summary>
    public static class InputGuideClassNames
    {
        public const string Root = "rector-input-guide";
        public const string Pad = Root + "__pad";
        public const string Keyboard = Root + "__keyboard";
        public const string Row = Root + "__row";
        public const string Cell = Root + "__cell";
        public const string CellLeft = Cell + "--left";
        public const string CellRight = Cell + "--right";
        public const string Chip = Root + "__chip";
        public const string ChipActive = Chip + "--active";
        public const string ChipDisabled = Chip + "--disabled";
        public const string Gap = Root + "__gap";
        public const string FaceOffset = Root + "__face-offset";
        public const string ShoulderGutter = Root + "__shoulder-gutter";
        public const string Key = Root + "__key";
        public const string KeyPlain = Key + "--plain";
        public const string Action = Root + "__action";
    }
}
