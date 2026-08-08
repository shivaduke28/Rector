using R3;

namespace Rector.UI.Settings
{
    /// <summary>
    /// 設定ページの1行。1行が1つの設定を持つ。
    /// 候補は表示文字列の並びとして持ち、選択はそのインデックスで扱う。
    /// インデックスから実際の値への変換はページモデルの責務。
    /// </summary>
    public interface ISettingRow
    {
        string Label { get; }

        /// <summary>行カーソルが乗っているか。</summary>
        ReactiveProperty<bool> IsFocused { get; }

        /// <summary>
        /// 行が上下/Submit/Cancelを占有しているか。メニューを開いている間だけtrueになり、
        /// ページのカーソル移動と「ページを閉じる」を行が横取りする。
        /// </summary>
        ReadOnlyReactiveProperty<bool> IsCapturingInput { get; }

        /// <param name="delta">右が+1、左が-1。</param>
        void OnHorizontal(int delta);

        /// <summary>占有中のみ呼ばれる。</summary>
        /// <param name="delta">リストを下へ進むのが+1、上へ戻るのが-1。</param>
        void OnVertical(int delta);

        void OnSubmit();

        /// <summary>占有中のみ呼ばれる。</summary>
        void OnCancel();
    }
}
