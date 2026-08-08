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
    /// 記号はDualShock前提(Xbox対応は設定オプションで後日)。
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

        const string DefaultParamLabel = "R1 PARAM";

        sealed class GuideContent
        {
            public string FaceTop;
            public string FaceLeft;
            public string FaceRight;
            public string FaceBottom;
            public bool Mute;

            /// <summary>R1セルの文言。nullなら無効(減光)。R1+✕のようなコンボもここで表す。</summary>
            public string Param;

            /// <summary>R1を握っていることが前提のステート(パラメータ表示中)で反転させる。</summary>
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
                Param = DefaultParamLabel,
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
                Param = "R1+✕ REPLACE",
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
                Param = DefaultParamLabel,
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

        public InputGuideView()
        {
            AddToClassList(UssClassName);
            pickingMode = PickingMode.Ignore;

            // セルは透明な位置決め箱で、背景の塗りは文字ぴったりの内側チップにだけ載せる
            var lockCell = CreateCell("L2 LOCK", CellLeftClassName, out lockChip);
            var grabCell = CreateCell("R2 GRAB", CellRightClassName, out grabChip);
            var muteCell = CreateCell("L1 MUTE", CellLeftClassName, out muteChip);
            var paramCell = CreateCell(DefaultParamLabel, CellRightClassName, out paramChip);
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
            ReadOnlyReactiveProperty<bool> visible)
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
                visible.Subscribe(x => style.display = x ? DisplayStyle.Flex : DisplayStyle.None)
            );
        }

        void UpdateContent()
        {
            var content = Contents[currentState];

            // ○はU+25CBだとJetBrains Monoにグリフが無く豆腐(□)になるので、U+25EFを使う
            SetFace(faceTop, "△", content.FaceTop);
            SetFace(faceLeft, "□", content.FaceLeft);
            SetFace(faceRight, "◯", content.FaceRight);
            SetFace(faceBottom, "✕", content.FaceBottom);

            SetChip(muteChip, content.Mute, false);
            paramChip.text = content.Param ?? DefaultParamLabel;
            SetChip(paramChip, content.Param != null, content.ParamActive);
            SetChip(grabChip, content.Grab, grabHeld);
            SetChip(lockChip, true, lockHeld);
        }

        /// <summary>空きポジションは「記号 -」の減光表示にして、何も無いことを見せる。</summary>
        static void SetFace(Label label, string symbol, string text)
        {
            label.text = text == null ? $"{symbol} -" : $"{symbol} {text}";
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
