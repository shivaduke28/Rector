using System;
using R3;
using UnityEngine.UIElements;

namespace Rector.UI.Settings
{
    /// <summary>
    /// 「ラベル 値」の1行。矢印もカレットも持たないので、操作できないことが見た目で分かる。
    /// 列の骨格は SettingRowUss を共有するので、値列の左端はステッパーやセレクターと揃う。
    /// </summary>
    public sealed class RectorSettingText : VisualElement
    {
        const string UssClassName = "rector-setting-text";

        readonly Label label = new();
        readonly Label valueLabel = new();

        public RectorSettingText()
        {
            AddToClassList(SettingRowUss.Row);
            AddToClassList(UssClassName);
            pickingMode = PickingMode.Ignore;

            label.AddToClassList(SettingRowUss.Label);
            Add(label);

            var value = new VisualElement { pickingMode = PickingMode.Ignore };
            value.AddToClassList(SettingRowUss.Value);
            valueLabel.AddToClassList(SettingRowUss.ValueLabel);
            value.Add(valueLabel);
            Add(value);
        }

        public IDisposable Bind(TextRowState state)
        {
            label.text = state.Label;
            return new CompositeDisposable(
                state.IsFocused.Subscribe(x => EnableInClassList(SettingRowUss.RowFocused, x)),
                state.Value.Subscribe(x => valueLabel.text = x));
        }
    }
}
