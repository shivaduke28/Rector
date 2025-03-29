using R3;
using UnityEngine.UIElements;

namespace Rector.UI.Graphs.Nodes
{
    public sealed class BeatNodeView : NodeView
    {
        public BeatNodeView(VisualElement templateContainer, BeatNode beatNode) : base(templateContainer, beatNode)
        {
            beatNode.Bpm.Subscribe(bpm => NameLabel.text = $"BPM {bpm:F0}").AddTo(Disposables);
        }
    }
}
