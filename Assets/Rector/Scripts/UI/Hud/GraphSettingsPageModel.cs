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
        readonly InputGuideSettings guideSettings;
        readonly ReactiveProperty<bool> isVisible = new(false);
        ReadOnlyReactiveProperty<bool> IButtonListPageModel.IsVisible => isVisible;

        readonly List<RectorButtonState> buttons = new();
        readonly List<(InputGuideMode Mode, RectorButtonState Button)> guideButtons = new();

        const int GroupButtonCount = NodeGroups.MaxCount - NodeGroups.MinCount + 1;

        Action onExit;
        int index;
        IDisposable disposable;

        public GraphSettingsPageModel(ButtonListPageView view, NodeGroups groups, InputGuideSettings guideSettings)
        {
            this.view = view;
            this.groups = groups;
            this.guideSettings = guideSettings;

            // ボタン順序: [Groups 1..8][Guide: Off][Guide: DualShock][Guide: Xbox]。
            // Enter()のカーソルシードとUpdateGroupHighlightはGroupsが先頭に並ぶ前提。
            for (var count = NodeGroups.MinCount; count <= NodeGroups.MaxCount; count++)
            {
                var value = count;
                buttons.Add(new RectorButtonState($"Groups: {value}", () => groups.SetCount(value)));
            }

            foreach (var mode in new[] { InputGuideMode.Off, InputGuideMode.DualShock, InputGuideMode.Xbox })
            {
                var value = mode;
                var button = new RectorButtonState($"Guide: {value}", () => guideSettings.SetMode(value));
                guideButtons.Add((value, button));
                buttons.Add(button);
            }
        }

        public void Initialize()
        {
            // 現在値はハイライトで示す。ButtonListPageViewはEnterのたびにボタンを
            // 作り直すが、RectorButtonStateは使い回されるので状態はここで持てる。
            disposable = new CompositeDisposable(
                view.Bind(this),
                groups.Count.Subscribe(UpdateGroupHighlight),
                guideSettings.Mode.Subscribe(UpdateGuideHighlight));
        }

        public void Dispose() => disposable?.Dispose();

        public void Enter(Action onExitAction)
        {
            onExit = onExitAction;

            // グループ数の現在値にカーソルを合わせて開く
            index = groups.CurrentCount - NodeGroups.MinCount;
            for (var i = 0; i < buttons.Count; i++)
            {
                buttons[i].IsFocused.Value = i == index;
            }

            isVisible.Value = true;
        }

        void UpdateGroupHighlight(int count)
        {
            // Guideボタンを巻き込まないようGroupsの区間だけを舐める
            for (var i = 0; i < GroupButtonCount; i++)
            {
                buttons[i].IsHighlighted.Value = i + NodeGroups.MinCount == count;
            }
        }

        void UpdateGuideHighlight(InputGuideMode mode)
        {
            foreach (var (value, button) in guideButtons)
            {
                button.IsHighlighted.Value = value == mode;
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
