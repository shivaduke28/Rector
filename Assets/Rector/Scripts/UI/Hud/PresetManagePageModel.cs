using System;
using System.Collections.Generic;
using R3;
using Rector.UI.Graphs.Serialization;

#nullable enable

namespace Rector.UI.Hud
{
    /// <summary>
    /// プリセットの保存と削除。準備のための画面なので、操作してもページから出ない。
    /// </summary>
    /// <remarks>
    /// 名前を付ける口はここに無い。保存は日時の既定名で作られ、名前を変えたくなったら
    /// [Open Preset Folder] で保存フォルダを開き、Finder の上でリネームする。
    /// 戻ってきてページに入り直せば新しい名前で並ぶ。
    ///
    /// 中身のあるプリセットはどれを押しても確認ダイアログを通す。かつて空の枠だけその場で
    /// 書き込むようにしていたら、行が「スロット」に見えるせいで押した瞬間にファイルができるのが
    /// 驚きになった。[Save as new] はその逆で、角括弧付きの動作の行であり、しかも新規作成なので
    /// 何も失わない。失うもののある Overwrite と Delete だけが確認を通る。
    ///
    /// UIの入力は Navigate/Submit/Cancel の3つしか無いので、削除だけを別のボタンに
    /// 割り当てることはできない。上書きと削除はダイアログの中で選ぶ。
    ///
    /// 行数は Finder 側でも変わるので、開くたび・戻るたびに組み直す。
    /// </remarks>
    public sealed class PresetManagePageModel : IInitializable, IDisposable, IButtonListPageModel
    {
        const string SaveAsNewLabel = "[Save as new]";
        const string OpenFolderLabel = "[Open Preset Folder]";

        readonly ButtonListPageView view;
        readonly GraphSaveManager graphSaveManager;
        readonly ConfirmDialogModel confirmDialog;
        readonly ReactiveProperty<bool> isVisible = new(false);
        readonly List<ButtonListItem> items = new();
        readonly List<RectorButtonState> buttons = new();

        /// <summary>今並んでいるプリセットの名前。行の位置を名前から引き直すために持つ。</summary>
        readonly List<string> presetNames = new();

        Action? onExit;
        IDisposable? disposable;
        int index;

        public PresetManagePageModel(ButtonListPageView view, GraphSaveManager graphSaveManager, ConfirmDialogModel confirmDialog)
        {
            this.view = view;
            this.graphSaveManager = graphSaveManager;
            this.confirmDialog = confirmDialog;
        }

        void IInitializable.Initialize() => disposable = view.Bind(this);

        void IDisposable.Dispose() => disposable?.Dispose();

        public void Enter(Action onExitAction)
        {
            onExit = onExitAction;
            index = 0;
            Refresh(null);
        }

        /// <summary>
        /// 一覧を取り直して組み直す。<paramref name="focusName"/> の行にカーソルを戻す。
        /// </summary>
        /// <remarks>
        /// 表示のオンオフを通すと <see cref="ButtonListPageView"/> が要素を組み直す。
        /// 同じ値の代入は流れないので、先に false を挟まないと古い行が残る。
        ///
        /// 順番が効く。組み直し → カーソル位置の確定 → 表示、の順でないとカーソルが消える。
        /// <see cref="RectorButton.Bind"/> は購読した時点の値を受け取るので、表示を先に立てると
        /// まだ誰にも立っていない IsFocused を読んでしまう。
        /// </remarks>
        void Refresh(string? focusName)
        {
            isVisible.Value = false;

            var previous = index;
            BuildRows();

            var found = focusName == null ? -1 : presetNames.IndexOf(focusName);
            // 一覧の先頭は [Save as new] なので、プリセットの n 番目は行の n+1 番目
            index = Math.Clamp(found >= 0 ? found + 1 : previous, 0, buttons.Count - 1);
            buttons[index].IsFocused.Value = true;
            isVisible.Value = true;
        }

        void BuildRows()
        {
            items.Clear();
            buttons.Clear();
            presetNames.Clear();

            Add(new RectorButtonState(SaveAsNewLabel, SaveAsNew));

            foreach (var info in graphSaveManager.GetAll())
            {
                var name = info.Name;
                presetNames.Add(name);
                Add(new RectorButtonState(PresetLabel.Row(info), () => Submit(name)));
            }

            Add(new RectorButtonState(OpenFolderLabel, OpenFolder));
        }

        void Add(RectorButtonState button)
        {
            buttons.Add(button);
            items.Add(ButtonListItem.Of(button));
        }

        /// <summary>今のグラフを新しいファイルに書く。名前は日時、変えたければ Finder で。</summary>
        void SaveAsNew()
        {
            var name = graphSaveManager.NextDefaultName();
            graphSaveManager.Save(name, out _);

            // 名前順に並ぶので、新しい行は末尾とは限らない。名前で引き直してカーソルを乗せる
            Refresh(name);
        }

        void Submit(string name)
        {
            // Finder 側で消えている・名前が変わっていることがある。その場合は一覧を取り直すだけ
            if (!graphSaveManager.TryGetInfo(name, out var info))
            {
                Refresh(null);
                return;
            }

            var choices = new[]
            {
                new ConfirmChoice("Overwrite", () => graphSaveManager.Save(info.Name, out _)),
                new ConfirmChoice("Delete", () => graphSaveManager.Delete(info.Name)),
            };

            // 押した行と同じ文面を出す。何を選んだかがそのまま確認の文になる
            isVisible.Value = false;
            confirmDialog.Enter(PresetLabel.Row(info), choices, () => Refresh(info.Name));
        }

        /// <summary>
        /// 保存フォルダを開く。ページはそのまま残す。
        /// </summary>
        /// <remarks>
        /// Finder でリネームしても一覧はすぐには変わらない。取り直す合図が無いので、
        /// ページに入り直したときに拾う。
        /// </remarks>
        void OpenFolder() => graphSaveManager.OpenDirectory();

        void Close()
        {
            buttons[index].IsFocused.Value = false;
            isVisible.Value = false;

            var callback = onExit;
            onExit = null;
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
