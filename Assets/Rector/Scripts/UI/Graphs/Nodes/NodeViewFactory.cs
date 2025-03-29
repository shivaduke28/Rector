namespace Rector.UI.Graphs.Nodes
{
    public sealed class NodeViewFactory
    {
        public NodeView Create(Node node)
        {
            switch (node)
            {
                case BeatNode beatNode:
                    var beatNodeView = new BeatNodeView(VisualElementFactory.Instance.CreateNode());
                    beatNodeView.Bind(beatNode);
                    return beatNodeView;
                default:
                    var nodeView = new NodeView(VisualElementFactory.Instance.CreateNode());
                    nodeView.Bind(node);
                    return nodeView;
            }
        }
    }
}
