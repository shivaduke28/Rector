using R3;
using UnityEngine.UIElements;

namespace Rector.UI.Graphs.Nodes
{
    /// <summary>
    /// active状態をカテゴリのfilledアイコンで表示する、源ノード用の標準View。
    /// node は IActiveStateNode を実装していること。
    /// </summary>
    public class ActiveSourceNodeView : NodeView
    {
        public ActiveSourceNodeView(VisualElement templateContainer, Node node) : base(templateContainer, node)
        {
            var activeStateNode = (IActiveStateNode)node;
            var icons = VisualElementFactory.Instance.Icons;
            var outline = icons.GetIcon(node.Category);
            if (icons.GetFilledIcon(node.Category) is { } filled)
            {
                activeStateNode.ActiveState
                    .Subscribe(active => Icon.style.backgroundImage = new StyleBackground(active ? filled : outline))
                    .AddTo(Disposables);
            }
        }
    }
}
