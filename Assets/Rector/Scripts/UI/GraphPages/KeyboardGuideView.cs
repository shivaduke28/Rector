using UnityEngine.UIElements;

namespace Rector.UI.GraphPages
{
    /// <summary>
    /// キーボード用の操作ガイド。パッドと違ってキーの物理配置を写しても意味が無いので、
    /// 押し方でまとめた3行に「キー名 動作」のチップを横へ流す。
    /// 1行目はビューを動かすキー、2行目は押しながら使う修飾キー、3行目は単押しのアクション。
    /// キーの縦揃えはあきらめる代わりに、縦の場所を大きく節約している。
    /// 行間は全部そろえて詰める。グループの切れ目は中身の性質で読めるので、
    /// 空きを足すより詰めた方が右下の占有が小さくて済む。
    /// 1行目のパン/ズーム/リセットはパッド版がスティック扱いで省いているもので、
    /// キーボードにはスティックが無くキー名を知らないと動かせないためここだけ載せる
    /// (移動のWASDは説明が要らないので出さない)。
    /// 2・3行目はパッド版と同じ内容を共有する(<see cref="InputGuideContents"/>)。
    /// 行の幅は文言で伸び縮みするので、右端を揃えてガイドの右辺だけは動かさない。
    /// </summary>
    public sealed class KeyboardGuideView : VisualElement
    {
        // 同じ動作に複数キーが割り当たっているものは併記する(RectorInput.inputactions と対応)
        const string PanKey = "IJKL";
        const string ZoomKey = "U/O";
        const string ResetKey = "P";
        const string LockKey = "TAB";
        const string ParamKey = "SHIFT";
        const string GrabKey = "ALT";
        const string FaceTopKey = "C";
        const string FaceLeftKey = "F";
        const string FaceRightKey = "X/ESC";
        const string FaceBottomKey = "Z/SPACE";
        const string MuteKey = "V";

        // 差し替え接続はSubmitとの同時押し。併記のまま「SHIFT+Z/SPACE」にすると長すぎるので主キーだけ書く
        const string ParamComboKey = ParamKey + "+Z";

        readonly InputGuideChip panChip;
        readonly InputGuideChip zoomChip;
        readonly InputGuideChip resetChip;
        readonly InputGuideChip lockChip;
        readonly InputGuideChip paramChip;
        readonly InputGuideChip grabChip;
        readonly InputGuideChip faceTop;
        readonly InputGuideChip faceLeft;
        readonly InputGuideChip faceRight;
        readonly InputGuideChip faceBottom;
        readonly InputGuideChip muteChip;

        public KeyboardGuideView()
        {
            AddToClassList(InputGuideClassNames.Keyboard);
            pickingMode = PickingMode.Ignore;

            // パン・ズーム・リセットはステートに依らず常に効くので、文言は作ったきり触らない
            var viewRow = CreateRow();
            panChip = new InputGuideChip(PanKey, "PAN");
            zoomChip = new InputGuideChip(ZoomKey, "ZOOM");
            resetChip = new InputGuideChip(ResetKey, "RESET");
            viewRow.Add(panChip);
            viewRow.Add(zoomChip);
            viewRow.Add(resetChip);
            Add(viewRow);

            var modifierRow = CreateRow();
            paramChip = new InputGuideChip(ParamKey);
            grabChip = new InputGuideChip(GrabKey, "GRAB");
            lockChip = new InputGuideChip(LockKey, InputGuideContents.LockLabel);
            modifierRow.Add(paramChip);
            modifierRow.Add(grabChip);
            modifierRow.Add(lockChip);
            Add(modifierRow);

            var actionRow = CreateRow();
            faceTop = new InputGuideChip(FaceTopKey);
            faceLeft = new InputGuideChip(FaceLeftKey);
            faceRight = new InputGuideChip(FaceRightKey);
            faceBottom = new InputGuideChip(FaceBottomKey);
            muteChip = new InputGuideChip(MuteKey, "MUTE");
            actionRow.Add(faceTop);
            actionRow.Add(faceLeft);
            actionRow.Add(faceRight);
            actionRow.Add(faceBottom);
            actionRow.Add(muteChip);
            Add(actionRow);
        }

        public void Apply(GuideContent content)
        {
            lockChip.SetAction(content.Lock);
            lockChip.SetState(true, false);
            grabChip.SetState(content.Grab, false);
            muteChip.SetState(content.Mute, false);

            paramChip.SetKey(content.ParamCombo ? ParamComboKey : ParamKey);
            paramChip.SetAction(content.Param ?? InputGuideContents.ParamLabel);
            paramChip.SetState(content.Param != null, content.ParamActive);

            SetFace(faceTop, content.FaceTop);
            SetFace(faceLeft, content.FaceLeft);
            SetFace(faceRight, content.FaceRight);
            SetFace(faceBottom, content.FaceBottom);
        }

        public void SetPressed(GuideInput input, bool value)
        {
            switch (input)
            {
                case GuideInput.FaceTop: faceTop.SetPressed(value); break;
                case GuideInput.FaceLeft: faceLeft.SetPressed(value); break;
                case GuideInput.FaceRight: faceRight.SetPressed(value); break;
                case GuideInput.FaceBottom: faceBottom.SetPressed(value); break;
                case GuideInput.UpperLeft: lockChip.SetPressed(value); break;
                case GuideInput.UpperRight: grabChip.SetPressed(value); break;
                case GuideInput.LowerLeft: muteChip.SetPressed(value); break;
                case GuideInput.LowerRight: paramChip.SetPressed(value); break;
                case GuideInput.Pan: panChip.SetPressed(value); break;
                case GuideInput.Zoom: zoomChip.SetPressed(value); break;
                case GuideInput.Reset: resetChip.SetPressed(value); break;
            }
        }

        /// <summary>空きポジションは「-」の減光表示にして、何も無いことを見せる。</summary>
        static void SetFace(InputGuideChip chip, string text)
        {
            chip.SetAction(text);
            chip.SetState(text != null, false);
        }

        static VisualElement CreateRow()
        {
            var row = new VisualElement { pickingMode = PickingMode.Ignore };
            row.AddToClassList(InputGuideClassNames.Row);
            return row;
        }
    }
}
