using System.Collections.Generic;
using System.Linq;
using Rector.UI.Nodes;
using UnityEngine;

namespace Rector.UI.Graphs
{
    public static class GraphSorter
    {
        public readonly struct Info
        {
            public readonly int LayerCount;
            public readonly int DummyNodeCount;
            public readonly int Type1ConflictCount;

            public Info(int dummyNodeCount, int layerCount, int type1ConflictCount)
            {
                DummyNodeCount = dummyNodeCount;
                LayerCount = layerCount;
                Type1ConflictCount = type1ConflictCount;
            }
        }

        public static Info Sort(IEnumerable<NodeView> nodeViews, IEnumerable<EdgeView> edgeViews)
        {
            // FIXME: 新規作成したノードが配列に置かれるように生成時にIndexInLayerを指定して、ここでソートしている
            // 富豪的なので負荷に問題がでたら修正する
            var unsortedNodes = nodeViews.OrderBy(x => x.LayerIndex * 100 + x.IndexInLayer).ToList();
            var edges = edgeViews.ToList();

            foreach (var edge in edges)
            {
                edge.DummyNodes.Clear();
            }

            // step1: layerを作る
            var (layers, layeredNodes) = ConstructLayers(unsortedNodes, edges);

            // step2: layer ごとに並び替え
            LayerOrderAssigner.AssignOrdering(layers);

            // step3: mark type 1 conflicts
            var markedEdges = MarkType1ConflictEdges(layers);

            // step4: Vertical alignment
            var (root, align) = CalculateVerticalAlignment(layers, markedEdges);

            // Alg. 3: Horizontal compaction
            var x = HorizontalCompaction(layers, layeredNodes, root, align);

            var position = new Vector2(60, 30);
            foreach (var layer in layers)
            {
                if (layer.Count == 0) continue;
                foreach (var node in layer)
                {
                    node.Position = new Vector2(x[node.Id] + position.x, position.y);
                }

                position.y += 80;
            }

            foreach (var edge in edges)
            {
                edge.Repaint();
            }

            // for (var i = 0; i < layers.Count; i++)
            // {
            //     Debug.Log($"---- layer {i} ----");
            //     var layer = layers[i];
            //     foreach (var node in layer)
            //     {
            //         Debug.Log(
            //             $"[{node.Index}] {node.Id.Value}, {node.Position}, {node.IsDummy},  align: {align[node.Id].Value}, root: {root[node.Id].Value}, sink: {x[node.Id]}");
            //     }
            // }

            return new Info(layers.Sum(x => x.Count(y => y.IsDummy)), layers.Count, markedEdges.Count);
        }

        /// <summary>
        /// ノードのx座標を計算する
        /// https://arxiv.org/abs/2008.01252 をそのまま実装した
        /// </summary>
        static Dictionary<NodeId, float> HorizontalCompaction(
            List<List<SortNode>> layers,
            Dictionary<NodeId, SortNode> layeredNodes,
            Dictionary<NodeId, NodeId> root,
            Dictionary<NodeId, NodeId> align)
        {
            var sink = new Dictionary<NodeId, NodeId>();
            var shift = new Dictionary<NodeId, float>();
            var x = new Dictionary<NodeId, float>();

            foreach (var layer in layers)
            {
                foreach (var node in layer)
                {
                    sink[node.Id] = node.Id;
                    shift[node.Id] = Mathf.Infinity;
                }
            }

            const float offset = 20f;

            foreach (var v in layeredNodes.Values)
            {
                if (root[v.Id] == v.Id)
                {
                    PlaceBlock(v);
                }
            }

            // 何やってるか理解してない...
            for (var layerIndex = 0; layerIndex < layers.Count; layerIndex++)
            {
                var layer = layers[layerIndex];
                if (layer.Count == 0) continue;
                var sinkNode = layer[0];
                if (sink[sinkNode.Id] == sinkNode.Id)
                {
                    if (float.IsInfinity(shift[sinkNode.Id])) shift[sinkNode.Id] = 0;
                    var j = layerIndex;
                    var k = 0;
                    while (true)
                    {
                        var v = layers[j][k];
                        while (align[v.Id] != root[v.Id])
                        {
                            v = layeredNodes[align[v.Id]];
                            j += 1;
                            if (v.Index > 0)
                            {
                                var u = layers[j][v.Index - 1];
                                shift[sink[u.Id]] = Mathf.Min(
                                    shift[sink[u.Id]],
                                    shift[sink[v.Id]] + x[v.Id] - (x[u.Id] + offset));
                            }
                        }

                        k = v.Index + 1;
                        if (k >= layers[j].Count || sink[v.Id] != sink[layers[j][k].Id]) break;
                    }
                }
            }

            foreach (var v in layeredNodes.Values)
            {
                x[v.Id] += shift[sink[v.Id]];
            }

            return x;

            // block内のnodeのxとsinkを決める
            // blockとsinkが共通のblockがある場合は先にそれを計算する（ので再起処理になってる）
            void PlaceBlock(SortNode rootNode)
            {
                if (x.TryAdd(rootNode.Id, 0f))
                {
                    var nodeInBlock = rootNode;
                    while (true)
                    {
                        if (nodeInBlock.Index > 0)
                        {
                            // 左にあるノード
                            var pred = layers[nodeInBlock.LayerIndex][nodeInBlock.Index - 1];
                            var rootOfPred = layeredNodes[root[pred.Id]];
                            PlaceBlock(rootOfPred);
                            if (sink[rootNode.Id] == rootNode.Id) sink[rootNode.Id] = sink[rootOfPred.Id];
                            if (sink[rootNode.Id] == sink[rootOfPred.Id])
                            {
                                // block内の全てのnodeに対してpred.Width + offsetだけ右にずらしたもののmaxを取ってる
                                x[rootNode.Id] = Mathf.Max(x[rootNode.Id], x[rootOfPred.Id] + pred.Width + offset);
                            }
                        }

                        // block内を下に進む
                        nodeInBlock = layeredNodes[align[nodeInBlock.Id]];
                        // 1周したらやめる
                        if (nodeInBlock.Id == rootNode.Id) break;
                    }

                    while (align[nodeInBlock.Id] != rootNode.Id)
                    {
                        nodeInBlock = layeredNodes[align[nodeInBlock.Id]];
                        x[nodeInBlock.Id] = x[rootNode.Id];
                        sink[nodeInBlock.Id] = sink[rootNode.Id];
                    }
                }
            }
        }

        /// <summary>
        /// Vertical Alignmentを計算する
        /// - nodeを複数のblockに分割する
        /// - blockの中で親は自分を親の中点とするような子をalignとしてもつ
        /// - blockの中で一番上のnodeをrootとしてもつ
        /// v0 = align[v2] = root[v0]=root[v1]=root[v2]
        /// v1 = align[v0]
        /// v2 = align[v1]
        /// </summary>
        static (Dictionary<NodeId, NodeId> root, Dictionary<NodeId, NodeId> align) CalculateVerticalAlignment(
            List<List<SortNode>> layers,
            HashSet<(NodeId up, NodeId down)> markedEdges)
        {
            var root = new Dictionary<NodeId, NodeId>();
            var align = new Dictionary<NodeId, NodeId>();

            foreach (var layer in layers)
            {
                foreach (var node in layer)
                {
                    root[node.Id] = node.Id;
                    align[node.Id] = node.Id;
                }
            }


            var temp = new List<SortNode>();
            foreach (var layer in layers)
            {
                var prevParentIndex = -1;
                foreach (var child in layer)
                {
                    var parentCount = child.Parents.Count;
                    if (parentCount == 0) continue;
                    temp.Clear();
                    temp.AddRange(child.Parents.Select(x => x.Node).OrderBy(x => x.Index));

                    // 親の中点が２つある場合は両方のalignを自分にするï
                    var d = child.Parents.Count - 1;
                    int m0;
                    int m1;
                    if (d % 2 == 0)
                    {
                        m0 = m1 = d / 2;
                    }
                    else
                    {
                        m0 = Mathf.FloorToInt(d / 2f);
                        m1 = m0 + 1;
                    }

                    for (var m = m0; m <= m1; m++)
                    {
                        var parent = temp[m];
                        if (align[child.Id] == child.Id)
                        {
                            if (!markedEdges.Contains((parent.Id, child.Id)) && prevParentIndex < parent.Index)
                            {
                                align[parent.Id] = child.Id;
                                root[child.Id] = root[parent.Id];
                                align[child.Id] = root[child.Id];
                                prevParentIndex = parent.Index;
                            }
                        }
                    }
                }
            }

            return (root, align);
        }

        /// <summary>
        /// Type 1 conflict (non-inner edgeがinner edgeと交差するケース）をmarkする
        /// inner edge同士の交差はすでに解消されている前提なので、片方がinnerならmarkしているのに注意
        /// </summary>
        /// <param name="layers"></param>
        /// <returns></returns>
        static HashSet<(NodeId up, NodeId down)> MarkType1ConflictEdges(List<List<SortNode>> layers)
        {
            var markedEdges = new HashSet<(NodeId up, NodeId down)>();
            for (var layerIndex = 1; layerIndex < layers.Count - 1; layerIndex++)
            {
                var prevInnerParent = 0;
                var childIndex = 1;
                var layerParent = layers[layerIndex];
                var layerChild = layers[layerIndex + 1];
                for (var innerChildIndex = 0; innerChildIndex < layerChild.Count; innerChildIndex++)
                {
                    var innerChild = layerChild[innerChildIndex];
                    var isInner = false;
                    SortNode innerParent = null;
                    if (innerChild.IsDummy)
                    {
                        var parent = innerChild.Parents[0].Node;
                        if (parent.IsDummy)
                        {
                            isInner = true;
                            innerParent = parent;
                        }
                    }

                    if (isInner || innerChildIndex == layerChild.Count - 1)
                    {
                        var innerParentIndex = isInner ? innerParent.Index : layerParent.Count - 1;
                        while (childIndex <= innerChildIndex)
                        {
                            var child = layerChild[childIndex];
                            foreach (var (parent, _) in child.Parents)
                            {
                                var parentIndex = parent.Index;
                                // childのindexの順番は保証されているので、parentのindexで交差判定をする
                                // 交差がない場合:
                                // prevInnerParent   parent    innerParent
                                //       |             |         |
                                // prevInnerChild    child     innerChild
                                if (parentIndex < prevInnerParent || parentIndex > innerParentIndex)
                                {
                                    markedEdges.Add((parent.Id, child.Id));
                                }
                            }

                            childIndex++;
                        }

                        prevInnerParent = innerParentIndex;
                    }
                }
            }

            return markedEdges;
        }

        static (List<List<SortNode>>, Dictionary<NodeId, SortNode>) ConstructLayers(List<NodeView> unsortedNodes,
            List<EdgeView> edges)
        {
            var layers = new List<List<SortNode>>();
            var layeredNodes = new Dictionary<NodeId, SortNode>();

            var islands = new List<SortNode>();
            var sources = new List<SortNode>();


            // step1: island
            for (var i = 0; i < unsortedNodes.Count;)
            {
                var node = unsortedNodes[i];
                var hasInput = node.Node.InputSlots.Any(x => x.ConnectedCount > 0);
                var hasOutput = node.Node.OutputSlots.Any(x => x.ConnectedCount > 0);
                if (!hasInput && !hasOutput)
                {
                    var genuineNode = new GenuineNode(node, 0);
                    islands.Add(genuineNode);
                    genuineNode.Index = islands.Count - 1;
                    layeredNodes.Add(node.Node.Id, genuineNode);
                    unsortedNodes.RemoveAt(i);
                }
                else
                {
                    i++;
                }
            }

            if (islands.Count > 0)
            {
                layers.Add(islands);
            }

            // source
            var sourceIndex = layers.Count;
            for (var i = 0; i < unsortedNodes.Count;)
            {
                var node = unsortedNodes[i];
                var hasInput = node.Node.InputSlots.Any(x => x.ConnectedCount > 0);
                if (!hasInput)
                {
                    var genuineNode = new GenuineNode(node, sourceIndex);
                    sources.Add(genuineNode);
                    genuineNode.Index = sources.Count - 1;
                    layeredNodes.Add(node.Node.Id, genuineNode);
                    unsortedNodes.RemoveAt(i);
                }
                else
                {
                    i++;
                }
            }

            if (sources.Count > 0)
            {
                layers.Add(sources);
            }

            // step2: それ以外のノードを最長パス法で layer に追加
            while (unsortedNodes.Count > 0)
            {
                var found = false;
                var layerIndex = layers.Count;
                var layer = new List<SortNode>();
                for (var i = 0; i < unsortedNodes.Count;)
                {
                    var node = unsortedNodes[i];
                    var edgesToNode = edges.Where(e => e.Edge.InputSlot.NodeId == node.Node.Id).ToArray();
                    // 全ての親がiよりも上のレイヤーなら追加してよい
                    if (edgesToNode.All(e => layeredNodes.TryGetValue(e.Edge.OutputSlot.NodeId, out var parent)
                                             && parent.LayerIndex < layerIndex))
                    {
                        var genuineNode = new GenuineNode(node, layerIndex);
                        // Debug.Log($"layer:{layerIndex}, {node.Name}");
                        layer.Add(genuineNode);
                        genuineNode.Index = layer.Count - 1;
                        layeredNodes.Add(node.Node.Id, genuineNode);
                        unsortedNodes.RemoveAt(i);

                        // dummy nodeを上から順に追加する
                        foreach (var edge in edgesToNode)
                        {
                            var outputNode = layeredNodes[edge.Edge.OutputSlot.NodeId];
                            var outputLayerIndex = outputNode.LayerIndex;

                            var parent = outputNode;
                            var parentSlotIndex = edge.Edge.OutputSlot.Index;
                            for (var j = outputLayerIndex + 1; j < layerIndex; j++)
                            {
                                var dummyNode = new DummyNode(NodeId.Generate(), j, edge.Edge.Id);
                                layers[j].Add(dummyNode);
                                dummyNode.Index = layers[j].Count - 1;
                                layeredNodes.Add(dummyNode.Id, dummyNode);
                                dummyNode.Parents.Add((parent, parentSlotIndex));
                                parent.Children.Add((dummyNode, 0));
                                // Debug.Log($"- add dummy node: {dummyNode.LayerIndex}");
                                edge.DummyNodes.Add(dummyNode);
                                parent = dummyNode;
                                parentSlotIndex = 0;
                            }

                            genuineNode.Parents.Add((parent, parentSlotIndex));
                            parent.Children.Add((genuineNode, edge.Edge.InputSlot.Index));
                        }

                        found = true;
                    }
                    else
                    {
                        i++;
                    }
                }

                if (!found)
                {
                    Debug.LogError("Cycle detected");
                    break;
                }

                layers.Add(layer);
            }

            return (layers, layeredNodes);
        }


        public abstract class SortNode
        {
            public abstract string Name { get; }
            public virtual bool IsDummy => false;
            public readonly NodeId Id;
            public readonly int LayerIndex;
            public readonly List<(SortNode Node, int SlotIndex)> Parents = new(0);
            public readonly List<(SortNode Node, int SlotIndex)> Children = new(0);
            public readonly int InputCount;
            public readonly int OutputCount;
            public abstract Vector2 Position { get; set; }
            public virtual int Index { get; set; }
            public abstract float Width { get; }

            protected SortNode(NodeId id, int layerIndex, int inputCount, int outputCount)
            {
                Id = id;
                LayerIndex = layerIndex;
                InputCount = inputCount;
                OutputCount = outputCount;
            }
        }

        public sealed class GenuineNode : SortNode
        {
            readonly NodeView node;
            public override float Width => node.Width;
            public override string Name => node.Node.Name;

            public override int Index
            {
                get => node.IndexInLayer;
                set => node.IndexInLayer = value;
            }

            public GenuineNode(NodeView node, int layerIndex) : base(node.Node.Id, layerIndex,
                node.Node.InputSlots.Length, node.Node.OutputSlots.Length)
            {
                this.node = node;
                this.node.LayerIndex = layerIndex;
            }

            public override Vector2 Position
            {
                get => node.Position;
                set => node.Position = value;
            }
        }

        public sealed class DummyNode : SortNode
        {
            public override string Name => "Dummy";
            public override bool IsDummy => true;
            public readonly EdgeId EdgeId;
            public override Vector2 Position { get; set; }
            public override float Width => 10f;

            public DummyNode(NodeId id, int layerIndex, EdgeId edgeId) : base(id, layerIndex, 1, 1)
            {
                EdgeId = edgeId;
            }
        }
    }
}
