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

            // ノード背景を値のスライダーとして使う。最初の子として挿すことで icon/label の下に描く
            var fill = new VisualElement();
            fill.AddToClassList("rector-node-value-fill");
            var content = Root.Q<VisualElement>("content");
            content.Insert(0, fill);

            midiNode.DisplayValue
                .Subscribe(value => fill.style.width = Length.Percent(Mathf.Clamp01(value) * 100f))
                .AddTo(Disposables);
        }
    }
}
