using R3;
using UnityEngine.UIElements;

namespace Rector.UI.Graphs.Nodes
{
    public sealed class BeatNodeView : NodeView
    {
        public BeatNodeView(VisualElement templateContainer) : base(templateContainer)
        {
        }

        public override void Bind(Node node)
        {
            base.Bind(node);
            if (node is BeatNode beatNode)
            {
                beatNode.Bpm.Subscribe(bpm => NameLabel.text = $"BPM {bpm:F0}").AddTo(Disposables);
            }
        }
    }
}