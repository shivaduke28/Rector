using System;
using System.Collections.Generic;
using R3;
using UnityEngine.UIElements;

namespace Rector.UI.Settings
{
    /// <summary>
    /// 「ラベル 値 ▼」の1行。Submitで値の下にメニューが開く。
    /// メニューは絶対配置で、開いても行そのものは動かさない
    /// (ステートで位置が動くと目で追えなくなるため)。
    /// </summary>
    /// <remarks>
    /// UI Toolkitに重ね順の指定は無く、階層の後ろにある要素ほど手前に描かれる。
    /// メニューを自分の子にすると下の行に潜ってしまうので、行より後ろに置かれた
    /// メニュー専用レイヤーへ預け、開くたびに行の座標へ合わせる。
    /// </remarks>
    public sealed class RectorSettingSelector : VisualElement
    {
        const string UssClassName = "rector-setting-selector";
        const string CaretClassName = UssClassName + "__caret";
        const string CaretExpandedClassName = CaretClassName + "--expanded";
        const string MenuClassName = UssClassName + "__menu";
        const string ItemClassName = UssClassName + "__item";
        const string ItemFocusedClassName = ItemClassName + "--focused";

        readonly Label label = new();
        readonly Label valueLabel = new();
        readonly Label caret = new("▼");
        readonly VisualElement valueColumn = new();
        readonly VisualElement menu = new();
        readonly List<Label> items = new();

        SelectorRowState state;

        public RectorSettingSelector()
        {
            AddToClassList(SettingRowUss.Row);
            AddToClassList(UssClassName);
            pickingMode = PickingMode.Ignore;

            label.AddToClassList(SettingRowUss.Label);
            Add(label);

            valueColumn.pickingMode = PickingMode.Ignore;
            valueColumn.AddToClassList(SettingRowUss.Value);
            valueLabel.AddToClassList(SettingRowUss.ValueLabel);
            caret.AddToClassList(CaretClassName);
            menu.AddToClassList(MenuClassName);
            menu.pickingMode = PickingMode.Ignore;
            valueColumn.Add(valueLabel);
            valueColumn.Add(caret);
            Add(valueColumn);

            // 行のレイアウトが決まる/変わるたびに合わせ直す。開いた瞬間だけ計算すると、
            // まだレイアウトが解決していないフレームで開かれたときに座標がNaNになる。
            RegisterCallback<GeometryChangedEvent>(_ => PlaceMenu());
        }

        /// <param name="menuLayer">
        /// 行より後ろに置かれた、行と同じ座標系のレイヤー。メニューはここに描く。
        /// </param>
        public IDisposable Bind(SelectorRowState rowState, VisualElement menuLayer)
        {
            state = rowState;
            label.text = rowState.Label;
            menuLayer.Add(menu);

            // 候補 → 確定値 → カーソル の順に張る。候補が入れ替わったあとで
            // 現在値とカーソルを塗り直せるよう、RebuildItemsも両方を呼び直す。
            return new CompositeDisposable(
                rowState.IsFocused.Subscribe(x => EnableInClassList(SettingRowUss.RowFocused, x)),
                rowState.Options.Subscribe(_ => RebuildItems()),
                rowState.SelectedIndex.Subscribe(_ => UpdateValue()),
                rowState.CursorIndex.Subscribe(_ => UpdateCursor()),
                rowState.IsExpanded.Subscribe(x =>
                {
                    if (x) PlaceMenu();
                    menu.style.display = x ? DisplayStyle.Flex : DisplayStyle.None;
                    caret.EnableInClassList(CaretExpandedClassName, x);
                }));
        }

        /// <summary>メニューを値の列の真下に合わせる。</summary>
        void PlaceMenu()
        {
            var rowLayout = layout;
            var valueLayout = valueColumn.layout;
            if (float.IsNaN(rowLayout.x) || float.IsNaN(valueLayout.x)) return;

            menu.style.left = rowLayout.x + valueLayout.x;
            menu.style.top = rowLayout.yMax;
            menu.style.width = valueLayout.width;
        }

        void RebuildItems()
        {
            menu.Clear();
            items.Clear();

            foreach (var text in state.Options.CurrentValue)
            {
                var item = new Label(text) { pickingMode = PickingMode.Ignore };
                item.AddToClassList(ItemClassName);
                menu.Add(item);
                items.Add(item);
            }

            UpdateValue();
            UpdateCursor();
        }

        // 現在値はメニューの上の値ラベルに出ているので、メニューの中では印を付けない
        void UpdateValue()
        {
            var options = state.Options.CurrentValue;
            var index = state.SelectedIndex.CurrentValue;
            valueLabel.text = index >= 0 && index < options.Count ? options[index] : string.Empty;
        }

        void UpdateCursor()
        {
            var index = state.CursorIndex.CurrentValue;
            for (var i = 0; i < items.Count; i++)
            {
                items[i].EnableInClassList(ItemFocusedClassName, i == index);
            }
        }
    }
}
