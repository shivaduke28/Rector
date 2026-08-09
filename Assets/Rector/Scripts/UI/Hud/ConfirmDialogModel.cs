using System;
using System.Collections.Generic;
using R3;

#nullable enable

namespace Rector.UI.Hud
{
    /// <summary>確認ダイアログの選択肢。Cancel はモデルが足すので、ここには危ない方だけ渡す。</summary>
    public readonly struct ConfirmChoice
    {
        public readonly string Label;
        public readonly Action Action;

        public ConfirmChoice(string label, Action action)
        {
            Label = label;
            Action = action;
        }
    }

    /// <summary>
    /// 取り消せない操作の前に一枚挟む確認ダイアログ。全ページで使い回す1インスタンス。
    /// </summary>
    /// <remarks>
    /// 呼ぶ側は<b>必ず自分を非表示にしてから</b><see cref="Enter"/>し、onClose で表示へ戻すこと。
    /// <see cref="UIInputAction.Register"/> は前のハンドラを黙って上書きし、ダイアログを閉じるときの
    /// Unregister が入力そのものを止めるので、呼び元を出したまま開くと「見えているのに操作できない
    /// ページ」ができる。表示のオンオフを通すと ButtonListPageView が入力を取り直す。
    ///
    /// 見出し行が使えるので、専用の View は持たず <see cref="ButtonListPageView"/> に相乗りする。
    /// </remarks>
    public sealed class ConfirmDialogModel : IInitializable, IDisposable, IButtonListPageModel
    {
        const string CancelLabel = "Cancel";

        readonly ButtonListPageView view;
        readonly ReactiveProperty<bool> isVisible = new(false);
        readonly List<ButtonListItem> items = new();
        readonly List<RectorButtonState> buttons = new();

        Action? onClose;
        IDisposable? disposable;
        int index;

        public ConfirmDialogModel(ButtonListPageView view)
        {
            this.view = view;
        }

        void IInitializable.Initialize() => disposable = view.Bind(this);

        void IDisposable.Dispose() => disposable?.Dispose();

        /// <param name="detail">対象の中身。空なら行ごと出さない。</param>
        public void Enter(string title, string detail, ConfirmChoice[] choices, Action onCloseAction)
        {
            onClose = onCloseAction;

            items.Clear();
            buttons.Clear();

            items.Add(ButtonListItem.Header(title));
            if (!string.IsNullOrEmpty(detail)) items.Add(ButtonListItem.Header(detail));

            // 先頭は必ず Cancel。開いた瞬間に危ない選択肢の上でカーソルが待っている状態を作らない
            Add(new RectorButtonState(CancelLabel, Close));
            foreach (var choice in choices)
            {
                var action = choice.Action;
                Add(new RectorButtonState(choice.Label, () => Run(action)));
            }

            index = 0;
            buttons[index].IsFocused.Value = true;
            isVisible.Value = true;
        }

        void Add(RectorButtonState button)
        {
            buttons.Add(button);
            items.Add(ButtonListItem.Of(button));
        }

        void Run(Action action)
        {
            // 先に実行してから畳む。呼び元はこのあとの onClose で表示へ戻り、結果を反映した行を組み直す
            action();
            Close();
        }

        void Close()
        {
            buttons[index].IsFocused.Value = false;
            isVisible.Value = false;

            // 1インスタンスの使い回しなので、次に開くまで前の戻り先を残さない
            var callback = onClose;
            onClose = null;
            callback?.Invoke();
        }

        IEnumerable<ButtonListItem> IButtonListPageModel.GetItems() => items;

        ReadOnlyReactiveProperty<bool> IButtonListPageModel.IsVisible => isVisible;

        void IButtonListPageModel.Submit() => buttons[index].OnClick();

        void IButtonListPageModel.Cancel() => Close();

        void IButtonListPageModel.Navigate(bool next)
        {
            buttons[index].IsFocused.Value = false;
            index = (index + (next ? 1 : -1) + buttons.Count) % buttons.Count;
            buttons[index].IsFocused.Value = true;
        }
    }
}
