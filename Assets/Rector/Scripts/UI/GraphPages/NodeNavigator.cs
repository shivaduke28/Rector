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

            if (direction is Direction.Left or Direction.Right)
            {
                // レイヤーはカラムを跨いで1行に連結されているので、端では折り返さずに止まる。
                // 折り返すとグラフの反対側へ飛び、表示位置も全幅スクロールしてしまう。
                var step = direction == Direction.Right ? 1 : -1;
                for (var i = currentLayer.IndexOf(current) + step; i >= 0 && i < currentLayer.Count; i += step)
                {
                    if (currentLayer[i] is LayeredNode layeredNode)
                    {
                        return layeredNode;
                    }
                }

                return current;
            }

            var up = direction == Direction.Up;

            // REMARK: Parents/Children はSortを実行しないと値が入らない情報なのに注意
            // Dummy Nodeを加味しているのでOfTypeでフィルタをする必要がある
            // 同一カラムの親子を最優先にする。カラム数が1なら従来と同じ挙動になる。
            var neighbor = (up ? current.Parents : current.Children)
                .Select(t => t.Node)
                .OfType<LayeredNode>()
                .OrderBy(x => Mathf.Abs(x.TargetPosition.x - current.TargetPosition.x))
                .FirstOrDefault();
            if (neighbor != null)
            {
                return neighbor;
            }

            // カラムを跨ぐエッジはParents/Childrenに入らないので、生のエッジ列から実の親子を辿る
            var crossing = FindAcrossColumns(current, up);
            if (crossing != null)
            {
                return crossing;
            }

            // 隣のレイヤーで一番x座標が近いノードに移動する。ノードのないレイヤーは飛ばす。
            for (var i = 0; i < layers.Count; i++)
            {
                currentLayerIndex = (currentLayerIndex + (up ? -1 : 1) + layers.Count) % layers.Count;
                var candidate = layers[currentLayerIndex]
                    .OfType<LayeredNode>()
                    .OrderBy(x => Mathf.Abs(x.Position.x - current.Position.x))
                    .FirstOrDefault();
                if (candidate != null)
                {
                    return candidate;
                }
            }

            return current;
        }

        LayeredNode FindAcrossColumns(LayeredNode current, bool up)
        {
            LayeredNode nearest = null;
            var nearestDistance = float.MaxValue;

            foreach (var edge in up ? current.EdgesToParent : current.EdgesToChild)
            {
                var e = edge.EdgeView.Edge;
                var nodeId = up ? e.OutputSlot.NodeId : e.InputSlot.NodeId;
                if (!graph.TryGetNode(nodeId, out var node)) continue;
                if (node.Column == current.Column) continue;

                var distance = Mathf.Abs(node.TargetPosition.x - current.TargetPosition.x);
                if (distance < nearestDistance)
                {
                    nearestDistance = distance;
                    nearest = node;
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
