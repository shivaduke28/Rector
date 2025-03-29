using System.Collections.Generic;
using Rector.UI.Graphs;
using Rector.UI.Graphs.Nodes;
using UnityEngine;

namespace Rector.UI.LayeredGraphDrawing
{
    public sealed class LayeredNode : ILayeredNode
    {
        public NodeId Id { get; }
        public bool IsDummy => false;
        public float Width => NodeView.Width;

        public Vector2 Position
        {
            get => NodeView.Position;
            set => NodeView.Position = value;
        }

        public int Layer { get; set; }
        public int Index { get; set; }
        public int InputSlotCount { get; }
        public int OutputSlotCount { get; }

        // 16は勘
        public List<(ILayeredNode Node, int SlotIndex)> Parents { get; } = new(16);
        public List<(ILayeredNode Node, int SlotIndex)> Children { get; } = new(16);

        public List<LayeredEdge> EdgesToParent { get; } = new(16);
        public List<LayeredEdge> EdgesToChild { get; } = new(16);

        public NodeView NodeView { get; }

        public LayeredNode(NodeView nodeView)
        {
            Id = nodeView.Node.Id;
            this.NodeView = nodeView;
            InputSlotCount = nodeView.Node.InputSlots.Length;
            OutputSlotCount = nodeView.Node.OutputSlots.Length;
        }
    }
}
