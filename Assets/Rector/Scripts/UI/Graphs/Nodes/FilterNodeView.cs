using R3;
using UnityEngine.UIElements;

namespace Rector.UI.Graphs.Nodes
{
    public sealed class FilterNodeView : NodeView
    {
        public FilterNodeView(VisualElement templateContainer, FilterNode filterNode) : base(templateContainer, filterNode)
        {
            filterNode.Value.Subscribe(v => NameLabel.text = $" == {v}").AddTo(Disposables);
        }
    }
}
