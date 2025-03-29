using Rector.UI.Graphs;

namespace Rector.UI.LayeredGraphDrawing
{
    public sealed class LayeredEdge
    {
        public EdgeId Id => EdgeView.Edge.Id;
        public EdgeView EdgeView { get; }

        public LayeredEdge(EdgeView edgeView)
        {
            EdgeView = edgeView;
        }
    }
}