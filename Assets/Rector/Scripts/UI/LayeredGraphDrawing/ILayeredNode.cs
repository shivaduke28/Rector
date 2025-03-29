using System.Collections.Generic;
using Rector.UI.Graphs;
using UnityEngine;

namespace Rector.UI.LayeredGraphDrawing
{
    public interface ILayeredNode
    {
        // static
        NodeId Id { get; }

        // static
        int InputSlotCount { get; }

        // static
        int OutputSlotCount { get; }

        // static
        bool IsDummy { get; }

        // nealy static
        float Width { get; }

        // dynamic
        Vector2 Position { get; set; }

        // dynamic
        int Layer { get; set; }

        // dynamic
        int Index { get; set; }

        // dynamic
        List<(ILayeredNode Node, int SlotIndex)> Parents { get; }

        // dynamic
        List<(ILayeredNode Node, int SlotIndex)> Children { get; }
    }
}
