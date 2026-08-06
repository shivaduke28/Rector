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
        /// フォーカスの移動はグループを跨ぐ。グループ内に閉じるとグループを跨いだ確認がしづらい。
        /// 左スティックの MoveGroup は「隣のグループへ一気に飛ぶ」ための別経路。
        /// </remarks>
        public LayeredNode SelectNextNode(LayeredNode current, Vector2 input)
        {
            var layers = graph.Layers;
            var direction = GetDirection(input);
            var currentLayerIndex = current.Layer;
            var currentLayer = layers[currentLayerIndex];

            if (direction is Direction.Left or Direction.Right)
            {
                // レイヤーは全グループを連結した1行。グループ境界では隣のグループへそのまま進み、
                // 行の端まで行ったら反対の端へ回り込む。
                var step = direction == Direction.Right ? 1 : -1;
                var count = currentLayer.Count;
                var start = currentLayer.IndexOf(current);
                for (var i = 1; i <= count; i++)
                {
                    var next = currentLayer[((start + step * i) % count + count) % count];
                    if (next is LayeredNode layeredNode)
                    {
                        return layeredNode;
                    }
                }

                return current;
            }

            var up = direction == Direction.Up;

            // REMARK: Parents/Children はSortを実行しないと値が入らない情報なのに注意
            // Dummy Nodeを加味しているのでOfTypeでフィルタをする必要がある
            // グループを跨ぐエッジはParents/Childrenに入らないので、まずは同一グループの親子から探す
            var neighbor = (up ? current.Parents : current.Children)
                .Select(t => t.Node)
                .OfType<LayeredNode>()
                .OrderBy(x => Mathf.Abs(x.TargetPosition.x - current.TargetPosition.x))
                .FirstOrDefault();
            if (neighbor != null)
            {
                return neighbor;
            }

            // グループを跨ぐエッジはParents/Childrenに入らないので、生のエッジ列から実の親子を辿る
            var crossing = FindAcrossGroups(current, up);
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

        LayeredNode FindAcrossGroups(LayeredNode current, bool up)
        {
            LayeredNode nearest = null;
            var nearestDistance = float.MaxValue;

            foreach (var edge in up ? current.EdgesToParent : current.EdgesToChild)
            {
                var e = edge.EdgeView.Edge;
                var nodeId = up ? e.OutputSlot.NodeId : e.InputSlot.NodeId;
                if (!graph.TryGetNode(nodeId, out var node)) continue;
                if (node.Group == current.Group) continue;

                var distance = Mathf.Abs(node.TargetPosition.x - current.TargetPosition.x);
                if (distance < nearestDistance)
                {
                    nearestDistance = distance;
                    nearest = node;
                }
            }

            return nearest;
        }

        /// <summary>
        /// 隣のグループのノードを返す。ノードのないグループは飛ばし、端まで行ったらループする。
        /// </summary>
        public LayeredNode FindNodeInAdjacentGroup(LayeredNode current, int direction, int groupCount)
        {
            var startGroup = current?.Group ?? 0;

            for (var step = 1; step <= groupCount; step++)
            {
                var group = ((startGroup + direction * step) % groupCount + groupCount) % groupCount;
                var candidate = FindNearestInGroup(group, current);
                if (candidate != null)
                {
                    return candidate;
                }
            }

            return null;
        }

        /// <summary>
        /// グループ内で、レイヤーが近く、次にx座標が近いノードを返す。
        /// </summary>
        LayeredNode FindNearestInGroup(int group, LayeredNode from)
        {
            LayeredNode nearest = null;
            var nearestLayerDistance = int.MaxValue;
            var nearestX = float.MaxValue;

            foreach (var layer in graph.Layers)
            {
                foreach (var node in layer)
                {
                    if (node is not LayeredNode layeredNode || layeredNode.Group != group) continue;

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
