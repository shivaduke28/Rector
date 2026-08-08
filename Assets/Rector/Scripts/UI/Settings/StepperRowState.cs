using System;
using System.Collections.Generic;
using R3;
using UnityEngine;

namespace Rector.UI.Settings
{
    /// <summary>
    /// 左右キーで候補を送り、送った先をその場で反映する行。
    /// グループ数やガイド表記のように、切り替えても画面が跳ねない設定に使う。
    /// </summary>
    /// <remarks>
    /// 端では巡回せずclampする。巡回すると押しっぱなしのまま8→1のように値が飛び、
    /// 押した長さと結果が結びつかなくなるため。端に着いたことは矢印の減光で見せる。
    /// </remarks>
    public sealed class StepperRowState : ISettingRow
    {
        public string Label { get; }
        public IReadOnlyList<string> Options { get; }
        public ReactiveProperty<bool> IsFocused { get; } = new(false);

        readonly ReactiveProperty<int> selectedIndex;
        public ReadOnlyReactiveProperty<int> SelectedIndex => selectedIndex;

        // ステッパーは入力を常にページへ返すので、占有することはない
        readonly ReactiveProperty<bool> isCapturingInput = new(false);
        ReadOnlyReactiveProperty<bool> ISettingRow.IsCapturingInput => isCapturingInput;

        readonly Action<int> onChanged;

        public StepperRowState(string label, IReadOnlyList<string> options, int selectedIndex, Action<int> onChanged)
        {
            Label = label;
            Options = options;
            this.selectedIndex = new ReactiveProperty<int>(Clamp(selectedIndex, options.Count));
            this.onChanged = onChanged;
        }

        /// <summary>設定の実体が外から変わったときに、onChangedを呼ばずに表示だけ合わせる。</summary>
        public void SetIndexWithoutNotify(int index) => selectedIndex.Value = Clamp(index, Options.Count);

        void ISettingRow.OnHorizontal(int delta)
        {
            var next = Clamp(selectedIndex.Value + Math.Sign(delta), Options.Count);
            if (next == selectedIndex.Value) return;

            selectedIndex.Value = next;
            onChanged(next);
        }

        void ISettingRow.OnVertical(int delta)
        {
        }

        void ISettingRow.OnSubmit()
        {
        }

        void ISettingRow.OnCancel()
        {
        }

        static int Clamp(int index, int count) => count == 0 ? 0 : Mathf.Clamp(index, 0, count - 1);
    }
}
