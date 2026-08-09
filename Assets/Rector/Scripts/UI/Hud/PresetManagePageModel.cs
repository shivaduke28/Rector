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
    /// 空の枠は失うものが無いのでそのまま書く。埋まっている枠は確認ダイアログへ回し、
    /// 上書きと削除をそこで選ばせる。UIの入力は Navigate/Submit/Cancel の3つしか無いので、
    /// 削除だけを別のボタンに割り当てることはできない。
    ///
    /// 行(<see cref="RectorButtonState"/>)は作り直さずテキストだけ差し替える。ダイアログから
    /// 戻ると ButtonListPageView が要素を組み直すが、同じ state を使い回していればカーソルが残る。
    /// </remarks>
    public sealed class PresetManagePageModel : IInitializable, IDisposable, IButtonListPageModel
    {
        const string HeaderText = "MANAGE PRESETS";

        readonly ButtonListPageView view;
        readonly GraphSaveManager graphSaveManager;
        readonly ConfirmDialogModel confirmDialog;
        readonly ReactiveProperty<bool> isVisible = new(false);
        readonly List<ButtonListItem> items = new();
        readonly List<RectorButtonState> buttons = new();

        Action? onExit;
        IDisposable? disposable;
        int index;

        public PresetManagePageModel(ButtonListPageView view, GraphSaveManager graphSaveManager, ConfirmDialogModel confirmDialog)
        {
            this.view = view;
            this.graphSaveManager = graphSaveManager;
            this.confirmDialog = confirmDialog;
        }

        void IInitializable.Initialize()
        {
            // 枠の数は固定なので行は一度きり。以降はテキストだけ差し替える
            items.Add(ButtonListItem.Header(HeaderText));
            foreach (var info in graphSaveManager.GetAllSlotInfo())
            {
                var slot = info.Number;
                var button = new RectorButtonState(PresetSlotLabel.Row(info), () => Submit(slot));
                buttons.Add(button);
                items.Add(ButtonListItem.Of(button));
            }

            disposable = view.Bind(this);
        }

        void IDisposable.Dispose() => disposable?.Dispose();

        public void Enter(Action onExitAction)
        {
            onExit = onExitAction;
            RefreshLabels();

            buttons[index].IsFocused.Value = false;
            index = 0;
            buttons[index].IsFocused.Value = true;
            isVisible.Value = true;
        }

        /// <summary>ダイアログから戻る先。カーソルは動かさず、操作の結果だけ拾い直す。</summary>
        void Resume()
        {
            RefreshLabels();
            isVisible.Value = true;
        }

        void RefreshLabels()
        {
            var infos = graphSaveManager.GetAllSlotInfo();
            for (var i = 0; i < buttons.Count; i++)
            {
                buttons[i].Text.Value = PresetSlotLabel.Row(infos[i]);
            }
        }

        void Submit(int slot)
        {
            var info = graphSaveManager.GetSlotInfo(slot);
            if (info.IsEmpty)
            {
                graphSaveManager.Save(slot, out _);
                RefreshLabels();
                return;
            }

            isVisible.Value = false;
            confirmDialog.Enter(
                PresetSlotLabel.Title(info),
                PresetSlotLabel.Detail(info),
                new[]
                {
                    new ConfirmChoice("Overwrite", () => graphSaveManager.Save(slot, out _)),
                    new ConfirmChoice("Delete", () => graphSaveManager.Delete(slot)),
                },
                Resume);
        }

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
