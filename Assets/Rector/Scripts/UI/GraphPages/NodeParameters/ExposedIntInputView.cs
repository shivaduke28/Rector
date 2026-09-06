using System;
using R3;
using UnityEngine.UIElements;

namespace Rector.UI.GraphPages.NodeParameters
{
    public sealed class ExposedIntInputView
    {
        readonly VisualElement root;
        readonly Label nameLabel;
        readonly RectorSliderInt slider;
        readonly Label valueLabel;

        // スライダーは ReactiveProperty を要求するので、スロットの裏が何であれ表示用の値を挟む。
        // 書き戻しはスロットの現在値と違うときだけ（BehaviorSubject 裏だと同値でも流れるので、素直に往復させると無限ループ）
        readonly ReactiveProperty<int> displayValue = new(0);
        RectorSliderIntState sliderState;

        public ExposedIntInputView(VisualElement container)
        {
            root = container.Q<VisualElement>("input");
            nameLabel = root.Q<Label>("name-label");
            slider = root.Q<RectorSliderInt>("slider");
            valueLabel = root.Q<Label>("value-label");
        }

        public IDisposable Bind(ExposedIntInputModel model)
        {
            var slot = model.Slot;
            nameLabel.text = model.Label;
            sliderState = new RectorSliderIntState(displayValue, slot.MinValue, slot.MaxValue);
            return new CompositeDisposable(
                slider.Bind(sliderState),
                slot.Observable().Subscribe(x =>
                {
                    displayValue.Value = x;
                    valueLabel.text = x.ToString();
                }),
                displayValue.Subscribe(x =>
                {
                    if (x != slot.Value) slot.Value = x;
                }),
                model.IsFocused.Subscribe(x => root.EnableInClassList("rector-exposed-input--focused", x))
            );
        }

        public void AddTo(VisualElement parent) => parent.Add(root);
    }
}
