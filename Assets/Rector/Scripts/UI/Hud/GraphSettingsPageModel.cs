using System;
using System.Collections.Generic;
using System.Linq;
using R3;
using Rector.UI.GraphPages;
using Rector.UI.LayeredGraphDrawing;
using Rector.UI.Settings;

namespace Rector.UI.Hud
{
    public sealed class GraphSettingsPageModel : IInitializable, IDisposable, ISettingsPageModel
    {
        static readonly InputGuideMode[] GuideModes = { InputGuideMode.Off, InputGuideMode.DualShock, InputGuideMode.Xbox, InputGuideMode.Keyboard };
        static readonly string[] GuideOptions = GuideModes.Select(x => x.ToString()).ToArray();

        static readonly string[] GroupCountOptions = Enumerable
            .Range(NodeGroups.MinCount, NodeGroups.MaxCount - NodeGroups.MinCount + 1)
            .Select(x => x.ToString())
            .ToArray();

        readonly SettingsPageView view;
        readonly NodeGroups groups;
        readonly InputGuideSettings guideSettings;
        readonly ReactiveProperty<bool> isVisible = new(false);
        ReadOnlyReactiveProperty<bool> ISettingsPageModel.IsVisible => isVisible;

        // グループ数は送るたびにグラフが組み変わるのが見えるのでステッパー、
        // ガイド表記は候補を並べて選ぶ方が分かりやすいのでメニューにする
        readonly StepperRowState groupCountRow;
        readonly SelectorRowState guideRow;
        readonly ISettingRow[] rows;

        Action onExit;
        IDisposable disposable;

        public GraphSettingsPageModel(SettingsPageView view, NodeGroups groups, InputGuideSettings guideSettings)
        {
            this.view = view;
            this.groups = groups;
            this.guideSettings = guideSettings;

            groupCountRow = new StepperRowState(
                "Group Count",
                GroupCountOptions,
                groups.CurrentCount - NodeGroups.MinCount,
                i => groups.SetCount(i + NodeGroups.MinCount));

            guideRow = new SelectorRowState("HUD Guide", i => guideSettings.SetMode(GuideModes[i]));

            rows = new ISettingRow[] { groupCountRow, guideRow };
        }

        public void Initialize() => disposable = view.Bind(this);

        public void Dispose() => disposable?.Dispose();

        public void Enter(Action onExitAction)
        {
            onExit = onExitAction;

            // 現在値は開くたびに読み直す。ここ以外から変わっていても表示が嘘にならない。
            groupCountRow.SetIndexWithoutNotify(groups.CurrentCount - NodeGroups.MinCount);
            guideRow.SetOptions(GuideOptions, Array.IndexOf(GuideModes, guideSettings.Mode.CurrentValue));

            isVisible.Value = true;
        }

        IReadOnlyList<ISettingRow> ISettingsPageModel.GetRows() => rows;

        void ISettingsPageModel.Cancel()
        {
            isVisible.Value = false;
            onExit?.Invoke();
            onExit = null;
        }
    }
}
