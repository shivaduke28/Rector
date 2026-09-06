using System.Collections.Generic;

namespace Rector.UI.GraphPages
{
    /// <summary>
    /// ステートごとの操作ガイドの中身。表記(パッド名/キー名)とレイアウトから切り離してあるので、
    /// <see cref="PadGuideView"/> と <see cref="KeyboardGuideView"/> が同じ表を見る。
    /// </summary>
    public sealed class GuideContent
    {
        public string FaceTop;
        public string FaceLeft;
        public string FaceRight;
        public string FaceBottom;
        public bool Mute;

        /// <summary>右下ボタンのセルの文言。nullなら無効(減光)。</summary>
        public string Param;

        /// <summary>下面ボタンとの同時押し(差し替え接続)。表記が「R1+✕」のようになる。</summary>
        public bool ParamCombo;

        /// <summary>右下ボタンを握っていることが前提のステート(パラメータ表示中)で反転させる。</summary>
        public bool ParamActive;

        public bool Grab;

        /// <summary>左上ボタン(L2/TAB)の文言。掴んでいる間だけ「子孫ごと移動」の修飾キーに変わる。</summary>
        public string Lock = InputGuideContents.LockLabel;
    }

    public static class InputGuideContents
    {
        public const string ParamLabel = "PARAM";
        public const string ReplaceLabel = "REPLACE";
        public const string LockLabel = "LOCK";

        /// <summary>LOCK/MUTE/GRAB と同じ4文字。左列は右寄せなので、長くするとボタン名が左へ押し出される。</summary>
        public const string TreeLabel = "TREE";

        static readonly Dictionary<GraphPageState, GuideContent> Contents = new()
        {
            [GraphPageState.NodeSelection] = new GuideContent
            {
                FaceTop = "ADD (DELETE)",
                FaceLeft = "ACTION",
                FaceRight = "(CUT)",
                FaceBottom = "SLOT",
                Mute = true,
                Param = ParamLabel,
                Grab = true,
            },
            [GraphPageState.SlotSelection] = new GuideContent
            {
                FaceLeft = "ACTION",
                FaceRight = "BACK (CUT)",
                FaceBottom = "TARGET",
                Mute = true,
            },
            [GraphPageState.TargetNodeSelection] = new GuideContent
            {
                FaceTop = "CONTINUE",
                FaceLeft = "ACTION",
                FaceRight = "BACK",
                FaceBottom = "SLOT",
                Mute = true,
            },
            [GraphPageState.TargetSlotSelection] = new GuideContent
            {
                FaceTop = "CONTINUE",
                FaceLeft = "ACTION",
                FaceRight = "BACK",
                FaceBottom = "CONNECT/CUT",
                Mute = true,
                Param = ReplaceLabel,
                ParamCombo = true,
            },
            [GraphPageState.NodeCreation] = new GuideContent
            {
                FaceRight = "BACK",
                FaceBottom = "OK",
            },
            [GraphPageState.NodeParameter] = new GuideContent
            {
                FaceLeft = "STEP",
                FaceRight = "CLOSE",
                Mute = true,
                Param = ParamLabel,
                ParamActive = true,
            },
        };

        // R2(ALT)で掴んでいる間のノード選択。△がコピーになり、長押し削除は効かない。
        // L2(TAB)は追従に加えて「左右で子孫ごと移動」の修飾キーになる
        static readonly GuideContent NodeSelectionGrab = new()
        {
            FaceTop = "COPY",
            FaceLeft = "ACTION",
            FaceRight = "(CUT)",
            FaceBottom = "SLOT",
            Mute = true,
            Param = ParamLabel,
            Grab = true,
            Lock = TreeLabel,
        };

        public static GuideContent Get(GraphPageState state, bool grabbing) =>
            grabbing && state == GraphPageState.NodeSelection ? NodeSelectionGrab : Contents[state];
    }
}
