using R3;
using UnityEngine.UIElements;

namespace Rector.UI.Graphs.Nodes
{
    public sealed class SequenceNodeView : NodeView
    {
        public SequenceNodeView(VisualElement templateContainer, SequenceNode sequenceNode) : base(templateContainer, sequenceNode)
        {
            sequenceNode.Beat.CombineLatest(sequenceNode.Length, (beat, len) => $"Seq {beat}/{len}")
                .Subscribe(text => NameLabel.text = text)
                .AddTo(Disposables);
        }
    }
}
