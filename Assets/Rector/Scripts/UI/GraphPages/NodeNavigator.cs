using System.Linq;
using Rector.UI.LayeredGraphDrawing;
using UnityEngine;

namespace Rector.UI.GraphPages
{
    public sealed class NodeNavigator
    {
        readonly LayeredGraph graph;

        public NodeNavigator(LayeredGraph graph)
        {
            this.graph = graph;
        }

        public LayeredNode SelectNextNode(LayeredNode current, Vector2 input)
        {
            var layers = graph.Layers;
            var direction = GetDirection(input);
            var currentLayerIndex = current.Layer;
            var currentLayer = layers[currentLayerIndex];
            var currentIndexInLayer = currentLayer.IndexOf(current);
            if (direction is Direction.Left or Direction.Right)
            {
                while (true)
                {
                    var nextIndex = currentIndexInLayer + (direction == Direction.Right ? 1 : -1);
                    nextIndex = (nextIndex + currentLayer.Count) % currentLayer.Count;
                    var next = currentLayer[nextIndex];
                    if (next is LayeredNode layeredNode)
                    {
                        return layeredNode;
                    }
                    currentIndexInLayer = nextIndex;
                }
            }
            else
            {
                while (true)
                {
                    var nextLayerIndex = currentLayerIndex + (direction == Direction.Up ? -1 : 1);
                    nextLayerIndex = (nextLayerIndex + layers.Count) % layers.Count;
                    var nextLayer = layers[nextLayerIndex];

                    // NOTE: 0番目のレイヤーは空の場合がある
                    if (nextLayer.Count != 0)
                    {
                        return nextLayer.Where(x => !x.IsDummy).Cast<LayeredNode>().OrderBy(x => Mathf.Abs(x.Position.x - current.Position.x)).First();
                    }
                    currentLayerIndex = nextLayerIndex;
                }
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
