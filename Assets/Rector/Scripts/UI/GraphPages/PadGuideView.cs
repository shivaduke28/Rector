using UnityEngine.UIElements;

namespace Rector.UI.GraphPages
{
    /// <summary>
    /// ゲームパッド用の操作ガイド。中心軸を挟んだ2カラムの固定グリッドに、
    /// ボタンの物理配置(L2/R2が上段、L1/R1が下段、△□○✕は菱形)を写す。
    /// 左セルは右揃えで軸に吸着、右セルは左揃え。△と✕は右カラム先頭なので記号のxが揃う。
    /// ステートで場所が動くと目で追えないので、構造と位置は常に固定のまま
    /// 文言と有効/無効(減光)・ホールド中の反転だけを切り替える。
    /// 空きボタンは「記号 -」で「何も無い」ことを見せる。
    /// ボタン名はDualShock/Xboxを設定で選ぶ(<see cref="InputGuideMode"/>)。位置は同じで表記だけ変わる。
    /// キーボード専用キーとスティック系(ズーム/パン/リセット)は載せない。
    /// </summary>
    public sealed class PadGuideView : VisualElement
    {
        /// <summary>
        /// パッドごとのボタン名。位置(上/左/右/下、ショルダー4種)は共通で、呼び名だけが違う。
        /// </summary>
        readonly struct ButtonNames
        {
            public readonly string FaceTop;
            public readonly string FaceLeft;
            public readonly string FaceRight;
            public readonly string FaceBottom;
            public readonly string UpperLeft;
            public readonly string UpperRight;
            public readonly string LowerLeft;
            public readonly string LowerRight;

            /// <summary>下面4ボタンが記号か。記号ならそれ自体がボタンの形なので枠で囲まない。</summary>
            public readonly bool FaceIsSymbol;

            public ButtonNames(string faceTop, string faceLeft, string faceRight, string faceBottom,
                string upperLeft, string upperRight, string lowerLeft, string lowerRight, bool faceIsSymbol)
            {
                FaceIsSymbol = faceIsSymbol;
                FaceTop = faceTop;
                FaceLeft = faceLeft;
                FaceRight = faceRight;
                FaceBottom = faceBottom;
                UpperLeft = upperLeft;
                UpperRight = upperRight;
                LowerLeft = lowerLeft;
                LowerRight = lowerRight;
            }
        }

        // ◯はU+25CBだとJetBrains Monoにグリフが無く豆腐(□)になるので、U+25EFを使う。
        // DualShockの下面は記号なので枠で囲まないが、Xboxは文字なので囲まないと動作名と混ざる
        static readonly ButtonNames DualShockNames = new("△", "□", "◯", "✕", "L2", "R2", "L1", "R1", true);
        static readonly ButtonNames XboxNames = new("Y", "X", "B", "A", "LT", "RT", "LB", "RB", false);

        readonly InputGuideChip lockChip;
        readonly InputGuideChip grabChip;
        readonly InputGuideChip muteChip;
        readonly InputGuideChip paramChip;
        readonly InputGuideChip faceTop;
        readonly InputGuideChip faceLeft;
        readonly InputGuideChip faceRight;
        readonly InputGuideChip faceBottom;

        ButtonNames names = DualShockNames;

        public PadGuideView()
        {
            AddToClassList(InputGuideClassNames.Pad);
            pickingMode = PickingMode.Ignore;

            // セルは透明な位置決め箱で、背景の塗りは文字ぴったりの内側チップにだけ載せる。
            // ショルダーの動作名は固定なので、ここで入れたら以後はボタン名しか触らない
            var lockCell = CreateCell(InputGuideClassNames.CellLeft, out lockChip, "LOCK");
            var grabCell = CreateCell(InputGuideClassNames.CellRight, out grabChip, "GRAB");
            var muteCell = CreateCell(InputGuideClassNames.CellLeft, out muteChip, "MUTE");
            var paramCell = CreateCell(InputGuideClassNames.CellRight, out paramChip);
            var faceTopCell = CreateCell(InputGuideClassNames.CellRight, out faceTop);
            var faceLeftCell = CreateCell(InputGuideClassNames.CellLeft, out faceLeft);
            var faceRightCell = CreateCell(InputGuideClassNames.CellRight, out faceRight);
            var faceBottomCell = CreateCell(InputGuideClassNames.CellRight, out faceBottom);

            // R側は△ボタンの右端あたりから始めて、L/R間に余白を作る
            Add(CreateRow(lockCell, CreateShoulderGutter(), grabCell));
            Add(CreateRow(muteCell, CreateShoulderGutter(), paramCell));

            var gap = new VisualElement { pickingMode = PickingMode.Ignore };
            gap.AddToClassList(InputGuideClassNames.Gap);
            Add(gap);

            // 菱形: △✕は中心軸の列、□は軸の左、○は軸から右へオフセット(パッドの形)
            Add(CreateRow(CreateBlankCell(), faceTopCell));
            var faceOffset = new VisualElement { pickingMode = PickingMode.Ignore };
            faceOffset.AddToClassList(InputGuideClassNames.FaceOffset);
            Add(CreateRow(faceLeftCell, faceOffset, faceRightCell));
            Add(CreateRow(CreateBlankCell(), faceBottomCell));
        }

        public void SetXbox(bool xbox) => names = xbox ? XboxNames : DualShockNames;

        public void Apply(GuideContent content)
        {
            SetFace(faceTop, names.FaceTop, content.FaceTop, names.FaceIsSymbol);
            SetFace(faceLeft, names.FaceLeft, content.FaceLeft, names.FaceIsSymbol);
            SetFace(faceRight, names.FaceRight, content.FaceRight, names.FaceIsSymbol);
            SetFace(faceBottom, names.FaceBottom, content.FaceBottom, names.FaceIsSymbol);

            lockChip.SetKey(names.UpperLeft);
            lockChip.SetState(true, false);
            grabChip.SetKey(names.UpperRight);
            grabChip.SetState(content.Grab, false);
            muteChip.SetKey(names.LowerLeft);
            muteChip.SetState(content.Mute, false);
            // 差し替え接続は下面ボタンとの同時押しなので、そのまま「R1+✕」と見せる
            paramChip.SetKey(content.ParamCombo ? $"{names.LowerRight}+{names.FaceBottom}" : names.LowerRight);
            paramChip.SetAction(content.Param ?? InputGuideContents.ParamLabel);
            paramChip.SetState(content.Param != null, content.ParamActive);
        }

        /// <summary>スティック側(Pan/Zoom/Reset)はこのレイアウトに載せていないので無視する。</summary>
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
            }
        }

        /// <summary>空きポジションは「-」の減光表示にして、何も無いことを見せる。</summary>
        static void SetFace(InputGuideChip chip, string button, string text, bool symbol)
        {
            chip.SetKey(button);
            chip.SetKeyFramed(!symbol);
            chip.SetAction(text);
            chip.SetState(text != null, false);
        }

        static VisualElement CreateCell(string sideClassName, out InputGuideChip chip, string actionText = "")
        {
            var cell = new VisualElement { pickingMode = PickingMode.Ignore };
            cell.AddToClassList(InputGuideClassNames.Cell);
            cell.AddToClassList(sideClassName);

            chip = new InputGuideChip(string.Empty, actionText);
            cell.Add(chip);
            return cell;
        }

        static VisualElement CreateBlankCell()
        {
            var cell = new VisualElement { pickingMode = PickingMode.Ignore };
            cell.AddToClassList(InputGuideClassNames.Cell);
            return cell;
        }

        static VisualElement CreateShoulderGutter()
        {
            var gutter = new VisualElement { pickingMode = PickingMode.Ignore };
            gutter.AddToClassList(InputGuideClassNames.ShoulderGutter);
            return gutter;
        }

        static VisualElement CreateRow(params VisualElement[] children)
        {
            var row = new VisualElement { pickingMode = PickingMode.Ignore };
            row.AddToClassList(InputGuideClassNames.Row);
            foreach (var child in children)
            {
                row.Add(child);
            }

            return row;
        }
    }
}
