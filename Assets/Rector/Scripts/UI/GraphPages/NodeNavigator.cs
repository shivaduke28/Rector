using System.Collections.Generic;
using System.Linq;
using Rector.UI.Graphs.Nodes;
using UnityEngine;

namespace Rector.UI.GraphPages
{
    public static class NodeNavigator
    {
        public static NodeView SelectNextNode(NodeView current, Vector2 input, List<List<NodeView>> layers)
        {
            var direction = GetDirection(input);
            var currentLayer = layers[current.LayerIndex];
            var currentIndex = currentLayer.IndexOf(current);
            if (direction is Direction.Left or Direction.Right)
            {
                var nextIndex = currentIndex + (direction == Direction.Right ? 1 : -1);
                nextIndex = (nextIndex + currentLayer.Count) % currentLayer.Count;
                return currentLayer[nextIndex];
            }
            else
            {
                var nextLayerIndex = current.LayerIndex + (direction == Direction.Up ? -1 : 1);
                nextLayerIndex = (nextLayerIndex + layers.Count) % layers.Count;
                var nextLayer = layers[nextLayerIndex];
                return nextLayer.OrderBy(x => Mathf.Abs(x.Position.x - current.Position.x)).First();
            }
        }

        enum Direction
        {
            Up,
            Down,
            Left,
            Right,
        }

        static Direction GetDirection(Vector2 input)
        {
            var absX = Mathf.Abs(input.x);
            var absY = Mathf.Abs(input.y);

            if (absX > absY)
            {
                return input.x > 0 ? Direction.Right : Direction.Left;
            }
            else
            {
                return input.y > 0 ? Direction.Up : Direction.Down;
            }
        }
    }
}
