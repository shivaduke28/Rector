using System;
using System.Collections.Generic;
using R3;
using Rector.UI.LayeredGraphDrawing;

namespace Rector.UI.Hud
{
    public sealed class GraphSettingsPageModel : IInitializable, IDisposable, IButtonListPageModel
    {
        readonly ButtonListPageView view;
        readonly NodeGroups groups;
        readonly ReactiveProperty<bool> isVisible = new(false);
        ReadOnlyReactiveProperty<bool> IButtonListPageModel.IsVisible => isVisible;

        readonly List<RectorButtonState> buttons = new();

        Action onExit;
        int index;
        IDisposable disposable;

        public GraphSettingsPageModel(ButtonListPageView view, NodeGroups groups)
        {
            this.view = view;
            this.groups = groups;

            for (var count = NodeGroups.MinCount; count <= NodeGroups.MaxCount; count++)
            {
                var value = count;
                buttons.Add(new RectorButtonState($"Groups: {value}", () => groups.SetCount(value)));
            }
        }

        public void Initialize()
        {
            // 現在のグループ数はハイライトで示す。ButtonListPageViewはEnterのたびにボタンを
            // 作り直すが、RectorButtonStateは使い回されるので状態はここで持てる。
            disposable = new CompositeDisposable(
                view.Bind(this),
                groups.Count.Subscribe(UpdateHighlight));
        }

        public void Dispose() => disposable?.Dispose();

        public void Enter(Action onExitAction)
        {
            onExit = onExitAction;

            // 現在の値にカーソルを合わせて開く
            index = groups.CurrentCount - NodeGroups.MinCount;
            for (var i = 0; i < buttons.Count; i++)
            {
                buttons[i].IsFocused.Value = i == index;
            }

            isVisible.Value = true;
        }

        void UpdateHighlight(int count)
        {
            for (var i = 0; i < buttons.Count; i++)
            {
                buttons[i].IsHighlighted.Value = i + NodeGroups.MinCount == count;
            }
        }

        IEnumerable<RectorButtonState> IButtonListPageModel.GetButtons() => buttons;

        void IButtonListPageModel.Submit() => buttons[index].OnClick();

        void IButtonListPageModel.Cancel()
        {
            buttons[index].IsFocused.Value = false;
            isVisible.Value = false;
            onExit?.Invoke();
            onExit = null;
        }

        void IButtonListPageModel.Navigate(bool next)
        {
            buttons[index].IsFocused.Value = false;

            index += next ? 1 : -1;
            index = (index + buttons.Count) % buttons.Count;

            buttons[index].IsFocused.Value = true;
        }
    }
}
