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

        /// <remarks>
        /// フォーカスの移動はカラム内で閉じる。カラムを跨ぐのは MoveColumn の担当。
        /// </remarks>
        public LayeredNode SelectNextNode(LayeredNode current, Vector2 input)
        {
            var layers = graph.Layers;
            var direction = GetDirection(input);
            var currentLayerIndex = current.Layer;
            var currentLayer = layers[currentLayerIndex];

            if (direction is Direction.Left or Direction.Right)
            {
                // レイヤーは全カラムを連結した1行なので、同じカラムのノードだけを拾う。
                // 端まで行ったらそのカラムの反対側へ回り込む。
                var step = direction == Direction.Right ? 1 : -1;
                var count = currentLayer.Count;
                var start = currentLayer.IndexOf(current);
                for (var i = 1; i <= count; i++)
                {
                    var next = currentLayer[((start + step * i) % count + count) % count];
                    if (next is LayeredNode layeredNode && layeredNode.Column == current.Column)
                    {
                        return layeredNode;
                    }
                }

                return current;
            }

            var up = direction == Direction.Up;

            // REMARK: Parents/Children はSortを実行しないと値が入らない情報なのに注意
            // Dummy Nodeを加味しているのでOfTypeでフィルタをする必要がある
            // カラムを跨ぐエッジはそもそもParents/Childrenに入らないので、ここは同一カラムに閉じる
            var neighbor = (up ? current.Parents : current.Children)
                .Select(t => t.Node)
                .OfType<LayeredNode>()
                .OrderBy(x => Mathf.Abs(x.TargetPosition.x - current.TargetPosition.x))
                .FirstOrDefault();
            if (neighbor != null)
            {
                return neighbor;
            }

            // 同じカラムの隣のレイヤーで一番x座標が近いノードに移動する。空のレイヤーは飛ばす。
            for (var i = 0; i < layers.Count; i++)
            {
                currentLayerIndex = (currentLayerIndex + (up ? -1 : 1) + layers.Count) % layers.Count;
                var candidate = layers[currentLayerIndex]
                    .OfType<LayeredNode>()
                    .Where(x => x.Column == current.Column)
                    .OrderBy(x => Mathf.Abs(x.Position.x - current.Position.x))
                    .FirstOrDefault();
                if (candidate != null)
                {
                    return candidate;
                }
            }

            return current;
        }

        /// <summary>
        /// 隣のカラムのノードを返す。ノードのないカラムは飛ばし、端まで行ったらループする。
        /// </summary>
        public LayeredNode FindNodeInAdjacentColumn(LayeredNode current, int direction, int columnCount)
        {
            var startColumn = current?.Column ?? 0;

            for (var step = 1; step <= columnCount; step++)
            {
                var column = ((startColumn + direction * step) % columnCount + columnCount) % columnCount;
                var candidate = FindNearestInColumn(column, current);
                if (candidate != null)
                {
                    return candidate;
                }
            }

            return null;
        }

        /// <summary>
        /// カラム内で、レイヤーが近く、次にx座標が近いノードを返す。
        /// </summary>
        LayeredNode FindNearestInColumn(int column, LayeredNode from)
        {
            LayeredNode nearest = null;
            var nearestLayerDistance = int.MaxValue;
            var nearestX = float.MaxValue;

            foreach (var layer in graph.Layers)
            {
                foreach (var node in layer)
                {
                    if (node is not LayeredNode layeredNode || layeredNode.Column != column) continue;

                    var layerDistance = from == null ? 0 : Mathf.Abs(layeredNode.Layer - from.Layer);
                    var x = from == null ? 0f : Mathf.Abs(layeredNode.Position.x - from.Position.x);

                    if (layerDistance < nearestLayerDistance || (layerDistance == nearestLayerDistance && x < nearestX))
                    {
                        nearest = layeredNode;
                        nearestLayerDistance = layerDistance;
                        nearestX = x;
                    }
                }
            }

            return nearest;
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
