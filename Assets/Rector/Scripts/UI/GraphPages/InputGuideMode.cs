namespace Rector.UI.GraphPages
{
    /// <summary>
    /// 操作ガイドの表記。パッドごとにボタン名が違うので、自動検出ではなく設定で選ぶ。
    /// Keyboardはボタン名だけでなくレイアウトごと別物になる。
    /// </summary>
    public enum InputGuideMode
    {
        Off,
        DualShock,
        Xbox,
        Keyboard,
    }
}
