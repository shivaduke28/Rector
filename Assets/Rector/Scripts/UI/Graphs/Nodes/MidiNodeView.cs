using R3;
using UnityEngine;
using UnityEngine.UIElements;

namespace Rector.UI.Graphs.Nodes
{
    public sealed class MidiNodeView : NodeView
    {
        public MidiNodeView(VisualElement templateContainer, MidiSourceNode midiNode) : base(templateContainer, midiNode)
        {
            midiNode.DisplayLabel.Subscribe(text => NameLabel.text = text).AddTo(Disposables);

            // 値は content 下端のゲージで示す。ラベルが黒0.2の背景を持つので、
            // 濃さを一定にするため最後の子として最前面に置く(バーは文字に被らない)
            var gauge = new VisualElement { pickingMode = PickingMode.Ignore };
            gauge.AddToClassList("rector-node-value-gauge");
            var content = Root.Q<VisualElement>("content");
            content.Add(gauge);

            midiNode.DisplayValue
                .Subscribe(value => gauge.style.width = Length.Percent(Mathf.Clamp01(value) * 100f))
                .AddTo(Disposables);
        }
    }
}
