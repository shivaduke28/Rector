using System;
using System.Collections.Generic;
using R3;
using Rector.UI.Graphs.Serialization;

#nullable enable

namespace Rector.UI.Hud
{
    public enum GraphSlotPageMode
    {
        Save,
        Load,
    }

    /// <summary>
    /// グラフの保存スロット一覧。保存と読み込みで同じ画面をモード違いで使う。
    /// </summary>
    /// <remarks>
    /// rector-page にタイトルの要素が無いので、どちらのモードで開いているかはボタンのラベルで示す。
    /// </remarks>
    public sealed class GraphSlotPageModel : IInitializable, IDisposable, IButtonListPageModel
    {
        readonly ButtonListPageView view;
        readonly GraphSaveManager graphSaveManager;
        readonly ReactiveProperty<bool> isVisible = new(false);
        readonly List<RectorButtonState> buttons = new();

        GraphSlotPageMode mode = GraphSlotPageMode.Load;

        /// <summary>上書きの確認待ちスロット。0なら確認待ちなし。Saveモードでのみ使う。</summary>
        int armedSlot;

        Action? onExit;
        IDisposable? disposable;
        int index;

        public GraphSlotPageModel(ButtonListPageView view, GraphSaveManager graphSaveManager)
        {
            this.view = view;
            this.graphSaveManager = graphSaveManager;
        }

        void IInitializable.Initialize() => disposable = view.Bind(this);

        void IDisposable.Dispose() => disposable?.Dispose();

        public void Enter(GraphSlotPageMode pageMode, Action onExitAction)
        {
            mode = pageMode;
            onExit = onExitAction;
            armedSlot = 0;

            // Viewが行を組み直すのは非表示->表示のときだけ。前のモードの行が残らないよう一度閉じる
            isVisible.Value = false;
            BuildRows();

            index = 0;
            buttons[index].IsFocused.Value = true;
            isVisible.Value = true;
        }

        /// <summary>Saveは全枠、Loadは中身のある枠だけ並べる。</summary>
        void BuildRows()
        {
            buttons.Clear();
            foreach (var info in graphSaveManager.GetAllSlotInfo())
            {
                if (mode == GraphSlotPageMode.Load && info.IsEmpty) continue;

                var slot = info.Number;
                buttons.Add(new RectorButtonState(ToLabel(info), () => Submit(slot)));
            }

            // 空リストは ButtonListPageView も Navigate も想定していない
            if (buttons.Count == 0) buttons.Add(new RectorButtonState("(no saved graphs)", () => { }));
        }

        /// <summary>Saveモード専用。Saveの行はスロット1..Nと1対1に並ぶ。</summary>
        void RefreshLabels()
        {
            var infos = graphSaveManager.GetAllSlotInfo();
            for (var i = 0; i < buttons.Count; i++)
            {
                buttons[i].Text.Value = ToLabel(infos[i]);
            }
        }

        string ToLabel(GraphSlotInfo info)
        {
            if (info.Number == armedSlot) return $"Overwrite Slot {info.Number}?   press again to confirm";

            var action = mode == GraphSlotPageMode.Save ? "Save to" : "Load";
            var detail = info.IsEmpty
                ? "(empty)"
                : $"{info.NodeCount} nodes / {info.EdgeCount} edges   {info.SavedAt}";

            return $"{action} Slot {info.Number}   {detail}";
        }

        void Submit(int slot)
        {
            if (mode == GraphSlotPageMode.Load)
            {
                if (graphSaveManager.Load(slot, out _)) Close();
                return;
            }

            // 中身のあるスロットはもう一度押させて上書きの意思を確かめる。undoが無いため
            if (armedSlot != slot && !graphSaveManager.GetSlotInfo(slot).IsEmpty)
            {
                armedSlot = slot;
                RefreshLabels();
                return;
            }

            armedSlot = 0;
            graphSaveManager.Save(slot, out _);
            RefreshLabels();
        }

        void Close()
        {
            buttons[index].IsFocused.Value = false;
            isVisible.Value = false;
            onExit?.Invoke();
            onExit = null;
        }

        IEnumerable<RectorButtonState> IButtonListPageModel.GetButtons() => buttons;

        ReadOnlyReactiveProperty<bool> IButtonListPageModel.IsVisible => isVisible;

        void IButtonListPageModel.Submit() => buttons[index].OnClick();

        void IButtonListPageModel.Cancel() => Close();

        void IButtonListPageModel.Navigate(bool next)
        {
            // 行を移ったら確認は無かったことにする
            if (armedSlot != 0)
            {
                armedSlot = 0;
                RefreshLabels();
            }

            buttons[index].IsFocused.Value = false;
            index = (index + (next ? 1 : -1) + buttons.Count) % buttons.Count;
            buttons[index].IsFocused.Value = true;
        }
    }
}
