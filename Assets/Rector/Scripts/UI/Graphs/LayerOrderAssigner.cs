using System;
using System.Collections.Generic;
using UnityEngine;

namespace Rector.UI.Graphs
{
    public static class LayerOrderAssigner
    {
        static readonly Comparison<GraphSorter.SortNode> ParentComparison = (a, b) =>
        {
            var aCenter = GetBaryCenter(a, true);
            var bCenter = GetBaryCenter(b, true);
            return BaryCenter.Compare(aCenter, bCenter);
        };

        static readonly Comparison<GraphSorter.SortNode> ChildComparison = (a, b) =>
        {
            var aCenter = GetBaryCenter(a, false);
            var bCenter = GetBaryCenter(b, false);
            return BaryCenter.Compare(aCenter, bCenter);
        };

        static BaryCenter GetBaryCenter(GraphSorter.SortNode node, bool useParent)
        {
            var neighbors = useParent ? node.Parents : node.Children;
            var nodeCenter = 0f;
            var slotCenter = 0f;
            var count = 0;
            var slotLeftMost = float.MaxValue;
            foreach (var (neighbor, slotIndex) in neighbors)
            {
                var i = neighbor.Index;
                nodeCenter += i;
                var slotPos = i + slotIndex / (float)(useParent ? neighbor.OutputCount : neighbor.InputCount);
                slotCenter += slotPos;
                slotLeftMost = Math.Min(slotLeftMost, slotPos);
                count++;
            }

            if (count == 0)
            {
                return new BaryCenter(node.Index, node.Index, node.IsDummy, node.Index, slotLeftMost);
            }

            return new BaryCenter(nodeCenter / count, slotCenter / count, node.IsDummy, node.Index, slotLeftMost);
        }

        /// <summary>
        /// レイヤーの順番を決定する
        /// SortNode.Indexに値が入っている前提のコードになっているのに注意
        /// </summary>
        public static void AssignOrdering(List<List<GraphSorter.SortNode>> layers)
        {
            // 何回か繰り返すとよいらしいので２往復させる
            SortByParent(layers);
            SortByChild(layers);
            SortByParent(layers);
            SortByChild(layers);
            SortByParent(layers);

            return;

            static void SortByParent(List<List<GraphSorter.SortNode>> layers)
            {
                // Debug.Log("--------------SortByParent-----------------");
                foreach (var layer in layers)
                {
                    // foreach (var node in layer)
                    // {
                    //     var bc = GetBaryCenter(node, true);
                    //     Debug.Log($"{node.Name}: slot={bc.slot}, leftMost:{bc.leftMost}");
                    // }

                    layer.Sort(ParentComparison);
                    for (var i = 0; i < layer.Count; i++)
                    {
                        layer[i].Index = i;
                    }
                }
            }

            static void SortByChild(List<List<GraphSorter.SortNode>> layers)
            {
                // Debug.Log("--------------SortByChild-----------------");
                for (var l = layers.Count - 1; l >= 0; l--)
                {
                    var layer = layers[l];
                    // foreach (var node in layer)
                    // {
                    //     var bc = GetBaryCenter(node, false);
                    //     Debug.Log($"{node.Name}: slot={bc.slot}, leftMost:{bc.leftMost}");
                    // }

                    layer.Sort(ChildComparison);
                    for (var i = 0; i < layer.Count; i++)
                    {
                        layer[i].Index = i;
                    }
                }
            }
        }


        readonly struct BaryCenter
        {
            // 繋がっているnodeの重心
            readonly float node;

            // 繋がっているslotの重心
            public readonly float slot;

            readonly bool isDummy;

            // 自信のindex（値が同じだった場合に使う）
            readonly int index;
            public readonly float leftMost;

            public BaryCenter(float node, float slot, bool isDummy, int index, float leftMost)
            {
                this.node = node;
                this.slot = slot;
                this.isDummy = isDummy;
                this.index = index;
                this.leftMost = leftMost;
            }

            public static int Compare(BaryCenter a, BaryCenter b)
            {
                var slot = a.slot.CompareTo(b.slot);
                if (slot != 0)
                {
                    return slot;
                }

                return a.leftMost.CompareTo(b.leftMost);
                // var n = a.node.CompareTo(b.node);
                // if (n != 0)
                // {
                //     return n;
                // }
                //
                // // genuine同士やdummy同士の場合はslotの重心を見る
                // if (a.isDummy == b.isDummy)
                // {
                //     var s = a.slot.CompareTo(b.slot);
                //     return s != 0 ? s : a.index.CompareTo(b.index);
                // }
                //
                // // genuineとdummyの場合はslotの重心を見ずにdummyを前におく
                // return (a.isDummy ? 0 : 1) - (b.isDummy ? 0 : 1);
            }
        }
    }
}
