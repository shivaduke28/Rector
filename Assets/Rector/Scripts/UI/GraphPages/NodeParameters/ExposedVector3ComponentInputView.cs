using System;
using R3;
using UnityEngine.UIElements;

namespace Rector.UI.GraphPages.NodeParameters
{
    /// <summary>
    /// Vector3の成分1つ分の行。レンジが無いのでスライダーは持たず、名前・刻み・値だけを出す。
    /// 列の位置はfloat行と揃えたいので、スライダーの場所には空きを置いてある。
    /// </summary>
    public sealed class ExposedVector3ComponentInputView
    {
        readonly VisualElement root;
        readonly Label nameLabel;
        readonly Label valueLabel;
        readonly Label stepLabel;

        public ExposedVector3ComponentInputView(VisualElement container)
        {
            root = container.Q<VisualElement>("input");
            nameLabel = root.Q<Label>("name-label");
            valueLabel = root.Q<Label>("value-label");
            stepLabel = root.Q<Label>("step-label");
        }

        public IDisposable Bind(ExposedVector3ComponentInputModel model)
        {
            nameLabel.text = model.Label;
            var format = model.ValueFormat;
            return new CompositeDisposable(
                model.Value.Subscribe(x => valueLabel.text = model.Read(x).ToString(format)),
                model.IsFocused.Subscribe(x => root.EnableInClassList("rector-exposed-input--focused", x)),
                model.StepType.Subscribe(x => stepLabel.text = ExposedStepCalculator.StepLabel(model.StepSize(x)))
            );
        }

        public void AddTo(VisualElement parent) => parent.Add(root);
    }
}
