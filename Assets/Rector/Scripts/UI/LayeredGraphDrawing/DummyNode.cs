using System.Collections.Generic;
using Rector.UI.Graphs;
using UnityEngine;

namespace Rector.UI.LayeredGraphDrawing
{
    public sealed class DummyNode : ILayeredNode
    {
        // 多分poolするように変更する
        public NodeId Id { get; }
        public bool IsDummy => true;
        public float Width => 10f;
        public Vector2 Position { get; set; }
        public int Layer { get; set; }
        public int Index { get; set; }
        public int InputSlotCount => 1;
        public int OutputSlotCount => 1;
        public List<(ILayeredNode Node, int SlotIndex)> Parents { get; } = new(1);
        public List<(ILayeredNode Node, int SlotIndex)> Children { get; } = new(1);

        public DummyNode(NodeId id)
        {
            Id = id;
        }
    }
}
