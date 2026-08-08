using System;
using System.Collections.Generic;
using R3;
using UnityEngine;

namespace Rector.UI.Settings
{
    /// <summary>
    /// Submitでメニューを開き、上下で候補を選んでもう一度Submitで確定する行。
    /// 解像度のように「送るたびに反映する」とウィンドウが跳ねてしまう設定に使う。
    /// メニューを閉じるまで値は適用しない。
    /// </summary>
    public sealed class SelectorRowState : ISettingRow
    {
        public string Label { get; }
        public ReactiveProperty<bool> IsFocused { get; } = new(false);

        readonly ReactiveProperty<IReadOnlyList<string>> options = new(Array.Empty<string>());
        public ReadOnlyReactiveProperty<IReadOnlyList<string>> Options => options;

        /// <summary>確定済みの値。まだ選ばれていないとき(候補が空、現在値が候補に無い)は-1。</summary>
        readonly ReactiveProperty<int> selectedIndex = new(-1);
        public ReadOnlyReactiveProperty<int> SelectedIndex => selectedIndex;

        /// <summary>メニューを開いている間のカーソル。確定せずに閉じたら捨てる。</summary>
        readonly ReactiveProperty<int> cursorIndex = new(0);
        public ReadOnlyReactiveProperty<int> CursorIndex => cursorIndex;

        readonly ReactiveProperty<bool> isExpanded = new(false);
        public ReadOnlyReactiveProperty<bool> IsExpanded => isExpanded;
        ReadOnlyReactiveProperty<bool> ISettingRow.IsCapturingInput => isExpanded;

        readonly Action<int> onCommit;

        public SelectorRowState(string label, Action<int> onCommit)
        {
            Label = label;
            this.onCommit = onCommit;
        }

        /// <summary>
        /// 候補と現在値を差し替える。解像度や入力デバイスは環境で変わるので、ページを開くたびに読み直す。
        /// </summary>
        /// <param name="selected">現在値の位置。まだ選ばれていなければ負の値を渡す。</param>
        public void SetOptions(IReadOnlyList<string> values, int selected)
        {
            isExpanded.Value = false;
            options.Value = values;
            // 選ばれていないときに先頭へ丸めると、選んでいない値を現在値として見せることになる
            selectedIndex.Value = values.Count == 0 || selected < 0 ? -1 : Mathf.Min(selected, values.Count - 1);
            cursorIndex.Value = Mathf.Max(selectedIndex.Value, 0);
        }

        void ISettingRow.OnHorizontal(int delta)
        {
            // 送るたびに反映されるのを避けるための行なので、左右では動かさない
        }

        void ISettingRow.OnVertical(int delta)
        {
            var count = options.Value.Count;
            if (count == 0) return;

            // 候補の多い解像度リストを端から端へ移れるよう、カーソルは巡回させる
            cursorIndex.Value = ((cursorIndex.Value + Math.Sign(delta)) % count + count) % count;
        }

        void ISettingRow.OnSubmit()
        {
            if (options.Value.Count == 0) return;

            if (!isExpanded.Value)
            {
                cursorIndex.Value = Mathf.Max(selectedIndex.Value, 0);
                isExpanded.Value = true;
                return;
            }

            isExpanded.Value = false;

            // 現在値を選び直しただけなら適用しない。同じ解像度でも適用すると画面が一度跳ねるため。
            if (cursorIndex.Value == selectedIndex.Value) return;

            selectedIndex.Value = cursorIndex.Value;
            onCommit(selectedIndex.Value);
        }

        void ISettingRow.OnCancel() => isExpanded.Value = false;
    }
}
