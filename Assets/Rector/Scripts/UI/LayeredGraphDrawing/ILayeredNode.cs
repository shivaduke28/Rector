using System.Collections.Generic;
using Rector.UI.Graphs;
using UnityEngine;

namespace Rector.UI.LayeredGraphDrawing
{
    public interface ILayeredNode
    {
        NodeId Id { get; }
        bool IsDummy { get; }
        float Width { get; }
        Vector2 Position { get; set; }
        int LayerIndex { get; set; }
        int IndexInLayer { get; set; }
        int InputSlotCount { get; }
        int OutputSlotCount { get; }

        // ソート中に値を入れる
        List<(ILayeredNode Node, int SlotIndex)> Parents { get; }
        List<(ILayeredNode Node, int SlotIndex)> Children { get; }
    }
}