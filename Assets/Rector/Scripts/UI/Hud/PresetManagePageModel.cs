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
    /// 名前を付ける口はここに無い。日時の既定名で作り、変えたければ Open Preset Folder から
    /// Finder でリネームする。失うもののある Overwrite と Delete だけ確認を挟む。
    ///
    /// UIの入力は Navigate/Submit/Cancel の3つしか無いので、削除だけを別のボタンに
    /// 割り当てることはできない。上書きと削除はダイアログの中で選ぶ。
    /// </remarks>
    public sealed class PresetManagePageModel : IInitializable, IDisposable, IButtonListPageModel
    {
        const string SaveAsNewLabel = "Save as new";
        const string OpenFolderLabel = "Open Preset Folder";

        readonly ButtonListPageView view;
        readonly GraphSaveManager graphSaveManager;
        readonly ConfirmDialogModel confirmDialog;
        readonly ReactiveProperty<bool> isVisible = new(false);
        readonly List<ButtonListItem> items = new();
        readonly List<RectorButtonState> buttons = new();

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

        /// <summary>一覧を取り直して組み直し、<paramref name="focusName"/> の行にカーソルを戻す。</summary>
        /// <remarks>
        /// 組み直し → カーソル → 表示 の順を崩さないこと。表示のオンオフで
        /// <see cref="ButtonListPageView"/> が要素を作り直し、<see cref="RectorButton.Bind"/> は
        /// 購読した時点の IsFocused を読むので、表示が先だとカーソルが消える。
        /// </remarks>
        void Refresh(string? focusName)
        {
            isVisible.Value = false;

            var previous = index;
            BuildRows();

            // 先頭が Save as new なので、プリセットの n 番目は行の n+1 番目
            var found = focusName == null ? -1 : presetNames.IndexOf(focusName);
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

            // 保存が無いときに余白が二重にならないよう、一覧がある側にだけ上の空きを持たせる
            var infos = graphSaveManager.GetAll();
            if (infos.Length > 0) items.Add(ButtonListItem.Spacer());

            foreach (var info in infos)
            {
                var name = info.Name;
                presetNames.Add(name);
                Add(new RectorButtonState(PresetLabel.Row(info), () => Submit(name)));
            }

            items.Add(ButtonListItem.Spacer());
            Add(new RectorButtonState(OpenFolderLabel, OpenFolder));
        }

        void Add(RectorButtonState button)
        {
            buttons.Add(button);
            items.Add(ButtonListItem.Of(button));
        }

        void SaveAsNew()
        {
            var name = graphSaveManager.NextDefaultName();
            graphSaveManager.Save(name, out _);

            // 名前順に並ぶので、新しい行は末尾とは限らない
            Refresh(name);
        }

        void Submit(string name)
        {
            // Finder 側で消えている・名前が変わっていることがある
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

        /// <summary>保存フォルダを開く。リネームを拾い直す合図は無いので、ページに入り直したときに反映される。</summary>
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
