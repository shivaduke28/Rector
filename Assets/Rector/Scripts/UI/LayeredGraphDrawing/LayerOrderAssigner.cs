using System;
using System.Collections.Generic;

namespace Rector.UI.LayeredGraphDrawing
{
    public static class LayerOrderAssigner
    {
        static readonly Comparison<ILayeredNode> ParentComparison = (a, b) =>
        {
            var aCenter = GetBaryCenter(a, true);
            var bCenter = GetBaryCenter(b, true);
            return BaryCenter.Compare(aCenter, bCenter);
        };

        static readonly Comparison<ILayeredNode> ChildComparison = (a, b) =>
        {
            var aCenter = GetBaryCenter(a, false);
            var bCenter = GetBaryCenter(b, false);
            return BaryCenter.Compare(aCenter, bCenter);
        };

        static BaryCenter GetBaryCenter(ILayeredNode node, bool useParent)
        {
            var neighbors = useParent ? node.Parents : node.Children;
            var slotCenter = 0f;
            var count = 0;
            var slotLeftMost = float.MaxValue;
            foreach (var (neighbor, slotIndex) in neighbors)
            {
                var i = neighbor.Index;
                var slotPos = i + slotIndex / (float)(useParent ? neighbor.OutputSlotCount : neighbor.InputSlotCount);
                slotCenter += slotPos;
                slotLeftMost = Math.Min(slotLeftMost, slotPos);
                count++;
            }

            if (count == 0)
            {
                return new BaryCenter(node.Index, slotLeftMost);
            }

            return new BaryCenter(slotCenter / count, slotLeftMost);
        }

        /// <summary>
        /// レイヤーの順番を決定する
        /// SortNode.Indexに値が入っている前提のコードになっているのに注意
        /// </summary>
        public static void AssignOrdering(List<List<ILayeredNode>> layers)
        {
            // 重心法(Sugiyama法の層内順序付け)は反復するほど交差が減る。
            // 2往復で見た目が安定したのでここで止めている
            SortByParent(layers);
            SortByChild(layers);
            SortByParent(layers);
            SortByChild(layers);
            SortByParent(layers);

            return;

            static void SortByParent(List<List<ILayeredNode>> layers)
            {
                foreach (var layer in layers)
                {
                    layer.Sort(ParentComparison);
                    for (var i = 0; i < layer.Count; i++)
                    {
                        layer[i].Index = i;
                    }
                }
            }

            static void SortByChild(List<List<ILayeredNode>> layers)
            {
                for (var l = layers.Count - 1; l >= 0; l--)
                {
                    var layer = layers[l];
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
            readonly float slot;
            readonly float leftMost;

            public BaryCenter(float slot, float leftMost)
            {
                this.slot = slot;
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
            }
        }
    }
}
