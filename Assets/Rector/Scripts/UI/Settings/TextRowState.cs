using R3;

namespace Rector.UI.Settings
{
    /// <summary>
    /// 値を見せるだけで操作を受け付けない行。設定そのものではなく、
    /// 設定の結果として決まる情報（受信中のアドレスなど）を並べるのに使う。
    /// </summary>
    /// <remarks>
    /// カーソルは他の行と同じように乗る。飛ばす作りにするとページ側の
    /// カーソル移動に「行の種類」の知識が要るので、乗るけれど何も起きない形にした。
    /// </remarks>
    public sealed class TextRowState : ISettingRow
    {
        public string Label { get; }
        public ReactiveProperty<bool> IsFocused { get; } = new(false);

        readonly ReactiveProperty<string> value;
        public ReadOnlyReactiveProperty<string> Value => value;

        readonly ReactiveProperty<bool> isCapturingInput = new(false);
        ReadOnlyReactiveProperty<bool> ISettingRow.IsCapturingInput => isCapturingInput;

        public TextRowState(string label, string text = "")
        {
            Label = label;
            value = new ReactiveProperty<string>(text);
        }

        /// <summary>表示を差し替える。他の行と同じく、読みはReadOnly・書きはメソッドで揃える。</summary>
        public void SetText(string text) => value.Value = text;

        void ISettingRow.OnHorizontal(int delta)
        {
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
    }
}
