using R3;
using UnityEngine.UIElements;

namespace Rector.UI.Graphs.Nodes
{
    public sealed class MidiNodeView : NodeView
    {
        public MidiNodeView(VisualElement templateContainer, MidiSourceNode midiNode) : base(templateContainer, midiNode)
        {
            midiNode.DisplayLabel.Subscribe(text => NameLabel.text = text).AddTo(Disposables);
        }
    }
}
