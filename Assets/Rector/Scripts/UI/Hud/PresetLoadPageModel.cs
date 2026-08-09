using System;
using System.Collections.Generic;
using R3;
using Rector.UI.Graphs.Serialization;

#nullable enable

namespace Rector.UI.Hud
{
    /// <summary>
    /// プリセットの読み込み。本番で使う画面なので、選んだら即その場で読んで閉じる。
    /// </summary>
    /// <remarks>
    /// 読み込みは今のグラフへ足すだけで何も失わないので確認は挟まない。
    /// 保存と削除は<see cref="PresetManagePageModel"/>。
    /// </remarks>
    public sealed class PresetLoadPageModel : IInitializable, IDisposable, IButtonListPageModel
    {
        readonly ButtonListPageView view;
        readonly GraphSaveManager graphSaveManager;
        readonly ReactiveProperty<bool> isVisible = new(false);
        readonly List<ButtonListItem> items = new();
        readonly List<RectorButtonState> buttons = new();

        Action? onExit;
        IDisposable? disposable;
        int index;

        public PresetLoadPageModel(ButtonListPageView view, GraphSaveManager graphSaveManager)
        {
            this.view = view;
            this.graphSaveManager = graphSaveManager;
        }

        void IInitializable.Initialize() => disposable = view.Bind(this);

        void IDisposable.Dispose() => disposable?.Dispose();

        public void Enter(Action onExitAction)
        {
            onExit = onExitAction;

            // 管理ページや Finder で増減するので、開くたびに組み直して先頭へ戻す
            BuildRows();
            index = 0;
            buttons[index].IsFocused.Value = true;
            isVisible.Value = true;
        }

        void BuildRows()
        {
            items.Clear();
            buttons.Clear();

            foreach (var info in graphSaveManager.GetAll())
            {
                var name = info.Name;
                var button = new RectorButtonState(PresetLabel.Row(info), () => Load(name));
                buttons.Add(button);
                items.Add(ButtonListItem.Of(button));
            }

            // 空リストは ButtonListPageView も Navigate も想定していない
            if (buttons.Count == 0)
            {
                var button = new RectorButtonState("(no presets)", () => { });
                buttons.Add(button);
                items.Add(ButtonListItem.Of(button));
            }
        }

        void Load(string name)
        {
            if (graphSaveManager.Load(name, out _)) Close();
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
