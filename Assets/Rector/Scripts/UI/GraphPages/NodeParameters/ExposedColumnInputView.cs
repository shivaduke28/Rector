using System;
using R3;
using UnityEngine.UIElements;

namespace Rector.UI.GraphPages.NodeParameters
{
    /// <summary>
    /// int入力と同じ見た目を使う。中身はスロットではなくカラム番号。
    /// </summary>
    public sealed class ExposedColumnInputView
    {
        readonly VisualElement root;
        readonly Label nameLabel;
        readonly RectorSliderInt slider;
        readonly Label valueLabel;

        RectorSliderIntState sliderState;

        public ExposedColumnInputView(VisualElement container)
        {
            root = container.Q<VisualElement>("input");
            nameLabel = root.Q<Label>("name-label");
            slider = root.Q<RectorSliderInt>("slider");
            valueLabel = root.Q<Label>("value-label");
        }

        public IDisposable Bind(ExposedColumnInputModel model)
        {
            nameLabel.text = ExposedColumnInputModel.Name;
            sliderState = new RectorSliderIntState(model.Value, model.MinValue, model.MaxValue);
            return new CompositeDisposable(
                slider.Bind(sliderState),
                model.Value.Subscribe(x => valueLabel.text = x.ToString()),
                model.IsFocused.Subscribe(x => root.EnableInClassList("rector-exposed-input--focused", x))
            );
        }

        public void AddTo(VisualElement parent) => parent.Add(root);
    }
}
