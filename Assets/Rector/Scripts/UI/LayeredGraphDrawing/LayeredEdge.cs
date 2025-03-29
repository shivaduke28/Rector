using System.Collections.Generic;
using Rector.UI.Graphs;

namespace Rector.UI.LayeredGraphDrawing
{
    public sealed class LayeredEdge
    {
        public EdgeId Id => EdgeView.Edge.Id;
        public EdgeView EdgeView { get; }
        public List<DummyNode> DummyNodes { get; } = new(8);

        public LayeredEdge(EdgeView edgeView)
        {
            EdgeView = edgeView;
        }

        public void Commit()
        {
            EdgeView.BendPoints.Clear();
            foreach (var dummyNode in DummyNodes)
            {
                EdgeView.BendPoints.Add(dummyNode.Position);
            }
            EdgeView.Repaint();
        }
    }
}
