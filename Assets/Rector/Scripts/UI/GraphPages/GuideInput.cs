namespace Rector.UI.GraphPages
{
    /// <summary>
    /// 操作ガイドの押下表示が対応する「位置」。どのアクションを見るかは
    /// <see cref="GraphInputAction"/> 側に閉じ込める(パッドとキーボードで同じ位置を指す)。
    /// </summary>
    public enum GuideInput
    {
        FaceTop,
        FaceLeft,
        FaceRight,
        FaceBottom,
        UpperLeft,
        UpperRight,
        LowerLeft,
        LowerRight,

        // 以下はキーボードのガイドにしか出ない(パッドはスティック)
        Pan,
        Zoom,
        Reset,
    }
}
