using R3;
using UnityEngine.UIElements;

namespace Rector.UI.Graphs.Nodes
{
    public sealed class EqualNodeView : NodeView
    {
        public EqualNodeView(VisualElement templateContainer, EqualNode equalNode) : base(templateContainer, equalNode)
        {
            equalNode.Value.Subscribe(v => NameLabel.text = $"Equal {v}").AddTo(Disposables);
        }
    }
}
