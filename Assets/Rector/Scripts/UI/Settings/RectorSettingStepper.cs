using System;
using R3;
using UnityEngine.UIElements;

namespace Rector.UI.Settings
{
    /// <summary>
    /// 「ラベル ◁ 値 ▷」の1行。これ以上送れない側の矢印を減光して行き止まりを見せる。
    /// </summary>
    public sealed class RectorSettingStepper : VisualElement
    {
        const string UssClassName = "rector-setting-stepper";
        const string ArrowClassName = UssClassName + "__arrow";
        const string ArrowDisabledClassName = ArrowClassName + "--disabled";

        readonly Label label = new();
        readonly Label valueLabel = new();
        readonly Label leftArrow = new("◁");
        readonly Label rightArrow = new("▷");

        public RectorSettingStepper()
        {
            AddToClassList(SettingRowUss.Row);
            AddToClassList(UssClassName);
            pickingMode = PickingMode.Ignore;

            label.AddToClassList(SettingRowUss.Label);
            Add(label);

            var value = new VisualElement { pickingMode = PickingMode.Ignore };
            value.AddToClassList(SettingRowUss.Value);
            leftArrow.AddToClassList(ArrowClassName);
            valueLabel.AddToClassList(SettingRowUss.ValueLabel);
            rightArrow.AddToClassList(ArrowClassName);
            value.Add(leftArrow);
            value.Add(valueLabel);
            value.Add(rightArrow);
            Add(value);
        }

        public IDisposable Bind(StepperRowState state)
        {
            label.text = state.Label;
            return new CompositeDisposable(
                state.IsFocused.Subscribe(x => EnableInClassList(SettingRowUss.RowFocused, x)),
                state.SelectedIndex.Subscribe(index =>
                {
                    valueLabel.text = index >= 0 && index < state.Options.Count ? state.Options[index] : string.Empty;
                    leftArrow.EnableInClassList(ArrowDisabledClassName, index <= 0);
                    rightArrow.EnableInClassList(ArrowDisabledClassName, index >= state.Options.Count - 1);
                }));
        }
    }
}
