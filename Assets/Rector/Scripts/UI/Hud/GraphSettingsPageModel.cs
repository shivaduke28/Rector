using System;
using System.Collections.Generic;
using R3;
using Rector.UI.GraphPages;
using Rector.UI.LayeredGraphDrawing;

namespace Rector.UI.Hud
{
    public sealed class GraphSettingsPageModel : IInitializable, IDisposable, IButtonListPageModel
    {
        readonly ButtonListPageView view;
        readonly NodeGroups groups;
        readonly GraphViewSettings viewSettings;
        readonly ReactiveProperty<bool> isVisible = new(false);
        ReadOnlyReactiveProperty<bool> IButtonListPageModel.IsVisible => isVisible;

        readonly List<RectorButtonState> buttons = new();

        Action onExit;
        int index;
        IDisposable disposable;

        readonly RectorButtonState followButton;

        public GraphSettingsPageModel(ButtonListPageView view, NodeGroups groups, GraphViewSettings viewSettings)
        {
            this.view = view;
            this.groups = groups;
            this.viewSettings = viewSettings;

            // 真偽値なのでハイライトではなくボタンの文言そのもので今の値を出す
            var follow = viewSettings.FollowSelectedNode;
            followButton = new RectorButtonState(string.Empty, () => follow.Value = !follow.Value);
            buttons.Add(followButton);

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
                groups.Count.Subscribe(UpdateHighlight),
                viewSettings.FollowSelectedNode.Subscribe(x => followButton.Text.Value = $"Follow Focus: {(x ? "On" : "Off")}"));
        }

        public void Dispose() => disposable?.Dispose();

        public void Enter(Action onExitAction)
        {
            onExit = onExitAction;

            // グループ数の現在値にカーソルを合わせて開く
            index = GroupButtonOffset + groups.CurrentCount - NodeGroups.MinCount;
            for (var i = 0; i < buttons.Count; i++)
            {
                buttons[i].IsFocused.Value = i == index;
            }

            isVisible.Value = true;
        }

        /// <summary>Groups ボタンの開始位置。手前に Follow Focus が1つ入っている。</summary>
        const int GroupButtonOffset = 1;

        void UpdateHighlight(int count)
        {
            for (var i = GroupButtonOffset; i < buttons.Count; i++)
            {
                buttons[i].IsHighlighted.Value = i - GroupButtonOffset + NodeGroups.MinCount == count;
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
