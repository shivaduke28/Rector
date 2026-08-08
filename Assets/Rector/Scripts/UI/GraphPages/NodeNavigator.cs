using System.Linq;
using Rector.UI.LayeredGraphDrawing;
using UnityEngine;

namespace Rector.UI.GraphPages
{
    public sealed class NodeNavigator
    {
        readonly LayeredGraph graph;
        readonly NodeGroups groups;

        public NodeNavigator(LayeredGraph graph, NodeGroups groups)
        {
            this.graph = graph;
            this.groups = groups;
        }

        /// <remarks>
        /// エッジは辿らず、視覚的な近さだけで行き先が決まる。
        /// 左右はグループ内の行を歩き、グループの端まで来たら隣のグループの最近傍へ入る。
        /// 上下はグループ内のレイヤー跨ぎを優先し、グループ内に縦の相手がいないときだけ
        /// 他グループも含めて近いレイヤーのノードへ移動する。
        /// </remarks>
        public LayeredNode SelectNextNode(LayeredNode current, Vector2 input)
        {
            var layers = graph.Layers;
            var direction = GetDirection(input);
            var currentLayerIndex = current.Layer;
            var currentLayer = layers[currentLayerIndex];

            if (direction is Direction.Left or Direction.Right)
            {
                var step = direction == Direction.Right ? 1 : -1;
                var group = groups.Fold(current.Group);

                // グループ内では同じ行を歩く。行は全グループを連結した1行なので、
                // 進んだ先の実ノードが同グループのうちは行内の隣がそのまま次のノード。
                var start = currentLayer.IndexOf(current);
                for (var i = start + step; i >= 0 && i < currentLayer.Count; i += step)
                {
                    if (currentLayer[i] is not LayeredNode inRow) continue;
                    if (groups.Fold(inRow.Group) == group) return inRow;
                    break; // 実ノードが別グループ = グループの端まで来た
                }

                // グループの端では行を続けず、隣のグループの最近傍(レイヤー優先、次にx)へ入る。
                // 隣のグループの同じ行にノードがいればレイヤー距離0で従来の行渡りと同じ遷移になり、
                // いないときだけ「遠くの行仲間」ではなく空間的に近いノードが選ばれる。
                var hop = FindNodeInAdjacentGroup(current, step, groups.CurrentCount);
                if (hop != null && groups.Fold(hop.Group) != group)
                {
                    return hop;
                }

                // 飛べる先が無い(実質1グループ)なら、従来通り行内でラップする
                return FindHorizontalInSameGroup(current, step) ?? current;
            }

            // 上下はまずグループ内のレイヤー跨ぎ。エッジは辿らない(視覚的な近さで行き先が決まる)。
            var up = direction == Direction.Up;
            var inGroup = FindVerticalInSameGroup(current, up);
            if (inGroup != null)
            {
                return inGroup;
            }

            // グループ内に縦の相手がいなければ、他グループも含めて隣のレイヤーから順に
            // x座標が一番近いノードへ移動する。ノードのないレイヤーは飛ばす。
            for (var i = 0; i < layers.Count - 1; i++)
            {
                currentLayerIndex = (currentLayerIndex + (up ? -1 : 1) + layers.Count) % layers.Count;
                var candidate = layers[currentLayerIndex]
                    .OfType<LayeredNode>()
                    .OrderBy(x => Mathf.Abs(x.TargetPosition.x - current.TargetPosition.x))
                    .FirstOrDefault();
                if (candidate != null)
                {
                    return candidate;
                }
            }

            return current;
        }

        /// <summary>
        /// 同じグループ内だけで、同じ行(レイヤー)の隣のノードを返す。グループ内の端まで
        /// 行ったらグループ内の反対端へ回り込む。行内に自分しかいなければnull。
        /// </summary>
        LayeredNode FindHorizontalInSameGroup(LayeredNode current, int direction)
        {
            var row = graph.Layers[current.Layer];
            var group = groups.Fold(current.Group);
            var count = row.Count;
            var start = row.IndexOf(current);

            for (var i = 1; i < count; i++)
            {
                var next = row[((start + direction * i) % count + count) % count];
                if (next is LayeredNode layeredNode && groups.Fold(layeredNode.Group) == group)
                {
                    return layeredNode;
                }
            }

            return null;
        }

        /// <summary>
        /// 同じグループ内だけで上下に移動する。隣のレイヤーから順に、グループ内のノードが
        /// いる最初のレイヤーでx座標が一番近いノードを返す。端まで行ったら反対側の端へ
        /// 回り込む。いなければnull。
        /// </summary>
        LayeredNode FindVerticalInSameGroup(LayeredNode current, bool up)
        {
            var layers = graph.Layers;
            var group = groups.Fold(current.Group);
            var layerIndex = current.Layer;

            // 自分のいるレイヤーは走査しない(layers.Count - 1回で他レイヤーを一巡)。
            // 含めると「グループ内に縦の他ノードがいない」ときに横並びの隣人やcurrent自身へ
            // 「上下移動」してしまう。
            for (var i = 0; i < layers.Count - 1; i++)
            {
                layerIndex = (layerIndex + (up ? -1 : 1) + layers.Count) % layers.Count;
                var candidate = layers[layerIndex]
                    .OfType<LayeredNode>()
                    .Where(x => groups.Fold(x.Group) == group)
                    .OrderBy(x => Mathf.Abs(x.TargetPosition.x - current.TargetPosition.x))
                    .FirstOrDefault();
                if (candidate != null)
                {
                    return candidate;
                }
            }

            return null;
        }

        /// <summary>
        /// 隣のグループのノードを返す。ノードのないグループは飛ばし、端まで行ったらループする。
        /// </summary>
        public LayeredNode FindNodeInAdjacentGroup(LayeredNode current, int direction, int groupCount)
        {
            var startGroup = current is null ? 0 : groups.Fold(current.Group);

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
                    if (node is not LayeredNode layeredNode || groups.Fold(layeredNode.Group) != group) continue;

                    var layerDistance = from == null ? 0 : Mathf.Abs(layeredNode.Layer - from.Layer);
                    // 他の判定と同じくアニメーション中の現在値ではなく確定値で比べる。
                    // Positionを使うと再レイアウト直後の0.2sだけ古い並びで一番近いノードを選んでしまう。
                    var x = from == null ? 0f : Mathf.Abs(layeredNode.TargetPosition.x - from.TargetPosition.x);

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
