using System;
using R3;
using UnityEngine.UIElements;

namespace Rector.UI.GraphPages.NodeParameters
{
    public sealed class ExposedBoolInputView
    {
        readonly VisualElement root;
        readonly Label nameLabel;
        readonly Label valueLabel;
        readonly RectorToggle toggle;

        public ExposedBoolInputView(VisualElement container)
        {
            root = container.Q<VisualElement>("input");
            nameLabel = root.Q<Label>("name-label");
            toggle = root.Q<RectorToggle>("toggle");
            valueLabel = root.Q<Label>("value-label");
        }

        public IDisposable Bind(ExposedBoolInputModel model)
        {
            var slot = model.Slot;
            nameLabel.text = model.Label;
            return new CompositeDisposable(
                toggle.Bind(model.ToggleState),
                model.IsFocused.Subscribe(x => root.EnableInClassList("rector-exposed-input--focused", x)),
                slot.Observable().Subscribe(x =>
                {
                    model.DisplayValue.Value = x;
                    valueLabel.text = x ? "true" : "false";
                }),
                // 書き戻しはスロットの現在値と違うときだけ（BehaviorSubject 裏だと同値でも流れるので、素直に往復させると無限ループ）
                model.DisplayValue.Subscribe(x =>
                {
                    if (x != slot.Value) slot.Value = x;
                })
            );
        }

        public void AddTo(VisualElement parent) => parent.Add(root);
    }
}
