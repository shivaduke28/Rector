using System;
using System.Collections.Generic;
using R3;
using UnityEngine.UIElements;

namespace Rector.UI.GraphPages
{
    /// <summary>
    /// グラフゾーン右下の操作ガイド。中心軸を挟んだ2カラムの固定グリッドに、
    /// ボタンの物理配置(L2/R2が上段、L1/R1が下段、△□○✕は菱形)を写す。
    /// 左セルは右揃えで軸に吸着、右セルは左揃え。△と✕は右カラム先頭なので記号のxが揃う。
    /// ステートで場所が動くと目で追えないので、構造と位置は常に固定のまま
    /// 文言と有効/無効(減光)・ホールド中の反転だけを切り替える。
    /// 空きボタンは「記号 -」で「何も無い」ことを見せる。
    /// ボタン名はDualShock/Xboxを設定で選ぶ(<see cref="InputGuideMode"/>)。位置は同じで表記だけ変わる。
    /// キーボード専用キーとスティック系(ズーム/パン/リセット)は載せない。
    /// </summary>
    public sealed class InputGuideView : VisualElement
    {
        const string UssClassName = "rector-input-guide";
        const string RowClassName = UssClassName + "__row";
        const string CellClassName = UssClassName + "__cell";
        const string CellLeftClassName = CellClassName + "--left";
        const string CellRightClassName = CellClassName + "--right";
        const string ChipClassName = UssClassName + "__chip";
        const string ChipActiveClassName = ChipClassName + "--active";
        const string ChipDisabledClassName = ChipClassName + "--disabled";
        const string GapClassName = UssClassName + "__gap";
        const string FaceOffsetClassName = UssClassName + "__face-offset";
        const string ShoulderGutterClassName = UssClassName + "__shoulder-gutter";

        const string ParamLabel = "PARAM";
        const string ReplaceLabel = "REPLACE";

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

            public ButtonNames(string faceTop, string faceLeft, string faceRight, string faceBottom,
                string upperLeft, string upperRight, string lowerLeft, string lowerRight)
            {
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

        // ◯はU+25CBだとJetBrains Monoにグリフが無く豆腐(□)になるので、U+25EFを使う
        static readonly ButtonNames DualShockNames = new("△", "□", "◯", "✕", "L2", "R2", "L1", "R1");
        static readonly ButtonNames XboxNames = new("Y", "X", "B", "A", "LT", "RT", "LB", "RB");

        sealed class GuideContent
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
        }

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

        readonly Label lockChip;
        readonly Label grabChip;
        readonly Label muteChip;
        readonly Label paramChip;
        readonly Label faceTop;
        readonly Label faceLeft;
        readonly Label faceRight;
        readonly Label faceBottom;

        GraphPageState currentState;
        bool grabHeld;
        bool lockHeld;
        ButtonNames names = DualShockNames;

        public InputGuideView()
        {
            AddToClassList(UssClassName);
            pickingMode = PickingMode.Ignore;

            // セルは透明な位置決め箱で、背景の塗りは文字ぴったりの内側チップにだけ載せる
            var lockCell = CreateCell(string.Empty, CellLeftClassName, out lockChip);
            var grabCell = CreateCell(string.Empty, CellRightClassName, out grabChip);
            var muteCell = CreateCell(string.Empty, CellLeftClassName, out muteChip);
            var paramCell = CreateCell(string.Empty, CellRightClassName, out paramChip);
            var faceTopCell = CreateCell(string.Empty, CellRightClassName, out faceTop);
            var faceLeftCell = CreateCell(string.Empty, CellLeftClassName, out faceLeft);
            var faceRightCell = CreateCell(string.Empty, CellRightClassName, out faceRight);
            var faceBottomCell = CreateCell(string.Empty, CellRightClassName, out faceBottom);

            // R側は△ボタンの右端あたりから始めて、L/R間に余白を作る
            Add(CreateRow(lockCell, CreateShoulderGutter(), grabCell));
            Add(CreateRow(muteCell, CreateShoulderGutter(), paramCell));

            var gap = new VisualElement { pickingMode = PickingMode.Ignore };
            gap.AddToClassList(GapClassName);
            Add(gap);

            // 菱形: △✕は中心軸の列、□は軸の左、○は軸から右へオフセット(パッドの形)
            Add(CreateRow(CreateBlankCell(), faceTopCell));
            var faceOffset = new VisualElement { pickingMode = PickingMode.Ignore };
            faceOffset.AddToClassList(FaceOffsetClassName);
            Add(CreateRow(faceLeftCell, faceOffset, faceRightCell));
            Add(CreateRow(CreateBlankCell(), faceBottomCell));
        }

        public IDisposable Bind(
            ReadOnlyReactiveProperty<GraphPageState> state,
            ReadOnlyReactiveProperty<bool> grabModifierHeld,
            ReadOnlyReactiveProperty<bool> lockHeldProperty,
            ReadOnlyReactiveProperty<InputGuideMode> mode)
        {
            return new CompositeDisposable(
                state.Subscribe(x =>
                {
                    currentState = x;
                    UpdateContent();
                }),
                grabModifierHeld.Subscribe(x =>
                {
                    grabHeld = x;
                    UpdateContent();
                }),
                lockHeldProperty.Subscribe(x =>
                {
                    lockHeld = x;
                    UpdateContent();
                }),
                mode.Subscribe(x =>
                {
                    style.display = x == InputGuideMode.Off ? DisplayStyle.None : DisplayStyle.Flex;
                    names = x == InputGuideMode.Xbox ? XboxNames : DualShockNames;
                    UpdateContent();
                })
            );
        }

        void UpdateContent()
        {
            var content = Contents[currentState];

            SetFace(faceTop, names.FaceTop, content.FaceTop);
            SetFace(faceLeft, names.FaceLeft, content.FaceLeft);
            SetFace(faceRight, names.FaceRight, content.FaceRight);
            SetFace(faceBottom, names.FaceBottom, content.FaceBottom);

            lockChip.text = $"{names.UpperLeft} LOCK";
            SetChip(lockChip, true, lockHeld);
            grabChip.text = $"{names.UpperRight} GRAB";
            SetChip(grabChip, content.Grab, grabHeld);
            muteChip.text = $"{names.LowerLeft} MUTE";
            SetChip(muteChip, content.Mute, false);
            // 差し替え接続は下面ボタンとの同時押しなので、そのまま「R1+✕」と見せる
            var paramPrefix = content.ParamCombo ? $"{names.LowerRight}+{names.FaceBottom}" : names.LowerRight;
            paramChip.text = $"{paramPrefix} {content.Param ?? ParamLabel}";
            SetChip(paramChip, content.Param != null, content.ParamActive);
        }

        /// <summary>空きポジションは「ボタン名 -」の減光表示にして、何も無いことを見せる。</summary>
        static void SetFace(Label label, string button, string text)
        {
            label.text = text == null ? $"{button} -" : $"{button} {text}";
            label.EnableInClassList(ChipDisabledClassName, text == null);
        }

        static void SetChip(Label chip, bool enabled, bool active)
        {
            chip.EnableInClassList(ChipDisabledClassName, !enabled);
            chip.EnableInClassList(ChipActiveClassName, enabled && active);
        }

        static VisualElement CreateCell(string text, string sideClassName, out Label chip)
        {
            var cell = new VisualElement { pickingMode = PickingMode.Ignore };
            cell.AddToClassList(CellClassName);
            cell.AddToClassList(sideClassName);

            chip = new Label(text) { pickingMode = PickingMode.Ignore };
            chip.AddToClassList(ChipClassName);
            cell.Add(chip);
            return cell;
        }

        static VisualElement CreateBlankCell()
        {
            var cell = new VisualElement { pickingMode = PickingMode.Ignore };
            cell.AddToClassList(CellClassName);
            return cell;
        }

        static VisualElement CreateShoulderGutter()
        {
            var gutter = new VisualElement { pickingMode = PickingMode.Ignore };
            gutter.AddToClassList(ShoulderGutterClassName);
            return gutter;
        }

        static VisualElement CreateRow(params VisualElement[] children)
        {
            var row = new VisualElement { pickingMode = PickingMode.Ignore };
            row.AddToClassList(RowClassName);
            foreach (var child in children)
            {
                row.Add(child);
            }

            return row;
        }
    }
}
