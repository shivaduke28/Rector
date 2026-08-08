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

            var gauge = new VisualElement();
            gauge.AddToClassList("rector-node-gauge");
            var fill = new VisualElement();
            fill.AddToClassList("rector-node-gauge__fill");
            gauge.Add(fill);

            var content = Root.Q<VisualElement>("content");
            Root.Insert(Root.IndexOf(content) + 1, gauge);

            midiNode.DisplayValue
                .Subscribe(value => fill.style.width = Length.Percent(Mathf.Clamp01(value) * 100f))
                .AddTo(Disposables);
        }
    }
}
