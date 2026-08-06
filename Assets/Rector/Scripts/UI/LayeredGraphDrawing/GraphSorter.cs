using System.Collections.Generic;
using System.Linq;
using Rector.UI.Graphs;
using UnityEngine;

namespace Rector.UI.LayeredGraphDrawing
{
    public sealed class GraphSorter
    {
        const float LayerHeight = 80f;

        readonly List<LayeredNode> unsortedNodes = new();
        readonly LayeredGraph graph;
        readonly GraphColumns columns;

        public GraphSorter(LayeredGraph graph, GraphColumns columns)
        {
            this.graph = graph;
            this.columns = columns;
        }

        public readonly struct SortResult
        {
            public readonly int LayerCount;
            public readonly int DummyNodeCount;
            public readonly int Type1ConflictCount;

            /// <summary>
            /// 幅が未解決(0)のNodeViewがあった。カラム幅を過小に見積もっているのでSortをやり直す。
            /// </summary>
            public readonly bool HasUnresolvedWidth;

            public SortResult(int dummyNodeCount, int layerCount, int type1ConflictCount, bool hasUnresolvedWidth)
            {
                DummyNodeCount = dummyNodeCount;
                LayerCount = layerCount;
                Type1ConflictCount = type1ConflictCount;
                HasUnresolvedWidth = hasUnresolvedWidth;
            }
        }

        public SortResult Sort()
        {
            unsortedNodes.Clear();

            foreach (var edge in graph.Edges.Values)
            {
                edge.DummyNodes.Clear();
            }

            foreach (var prevLayer in graph.Layers)
            {
                foreach (var node in prevLayer)
                {
                    node.Parents.Clear();
                    node.Children.Clear();
                    if (node is LayeredNode layeredNode)
                    {
                        unsortedNodes.Add(layeredNode);
                    }
                }
            }

            // レンジ外のカラムを持つノードはどのカラムにも切り出されず、グラフから見えなくなる。
            // 通常はカラム数の変更時にLayeredGraph側で寄せているが、保険としてここでも締める。
            var lastColumn = columns.CurrentCount - 1;
            foreach (var node in unsortedNodes)
            {
                node.Column = Mathf.Clamp(node.Column, 0, lastColumn);
            }

            // step1: layerを作る
            // 層決めはカラムを跨ぐエッジも含めた全DAGで行う。これで親の層 < 子の層が常に成り立ち、
            // カラムを跨ぐエッジは必ず下向きになる。
            var (layers, layeredNodes) = ConstructLayers(unsortedNodes);

            // 層 -> y。空のレイヤーはyを進めない（カラム分割前と同じ規則）。
            // 全カラムで共有するので、ここで一度だけ作る。
            var layerY = new float[layers.Count];
            {
                var y = 0f;
                for (var i = 0; i < layers.Count; i++)
                {
                    layerY[i] = y;
                    if (layers[i].Count > 0) y += LayerHeight;
                }
            }

            var columnCount = columns.CurrentCount;
            var columnLayers = new List<List<ILayeredNode>>[columnCount];
            columns.BeginLayout();

            var dummyNodeCount = 0;
            var type1ConflictCount = 0;
            var hasUnresolvedWidth = false;
            var originX = 0f;

            // step2以降はカラムごとに独立して回す。これで並び替えとx圧縮の影響がカラム内に閉じる。
            for (var c = 0; c < columnCount; c++)
            {
                var (colLayers, colNodes) = SplitColumn(layers, layeredNodes, c);
                columnLayers[c] = colLayers;

                // step2: layer ごとに並び替え
                LayerOrderAssigner.AssignOrdering(colLayers);

                // step3: mark type 1 conflicts
                var markedEdges = MarkType1ConflictEdges(colLayers);

                // step4: Vertical alignment
                var (root, align) = CalculateVerticalAlignment(colLayers, markedEdges);

                // Alg. 3: Horizontal compaction
                var x = HorizontalCompaction(colLayers, colNodes, root, align);

                var minX = 0f;
                var contentWidth = 0f;
                if (colNodes.Count > 0)
                {
                    minX = float.MaxValue;
                    var maxX = float.MinValue;
                    foreach (var node in colNodes.Values)
                    {
                        var nodeX = x[node.Id];
                        minX = Mathf.Min(minX, nodeX);
                        maxX = Mathf.Max(maxX, nodeX + node.Width);
                        // NodeView.WidthはresolvedStyle由来なので、追加直後のフレームでは0のことがある。
                        // そのまま確定するとカラム幅が足りず、ノードが右のborderをはみ出す。
                        if (!node.IsDummy && node.Width <= 0f) hasUnresolvedWidth = true;
                    }

                    contentWidth = maxX - minX;
                }

                var width = columns.Place(originX, contentWidth);

                foreach (var node in colNodes.Values)
                {
                    node.Position = new Vector2(x[node.Id] - minX + originX + GraphColumns.Padding, layerY[node.Layer]);
                    if (node.IsDummy) dummyNodeCount++;
                }

                // カラム同士は隙間なく並べる。区切りは左borderと内側のPaddingで付ける。
                originX += width;
                type1ConflictCount += markedEdges.Count;
            }

            // commit dummy node positions to edges
            foreach (var edge in graph.Edges.Values)
            {
                edge.Commit();
            }

            // graph.Layers は「カラム順 -> カラム内Index順」で連結する。
            // 1行が左から右に並んだノード列であり続けるので、NodeNavigatorの左右移動が
            // そのままカラムを跨ぐ移動になる。
            graph.Layers.Clear();
            for (var i = 0; i < layers.Count; i++)
            {
                var merged = new List<ILayeredNode>();
                for (var c = 0; c < columnCount; c++)
                {
                    merged.AddRange(columnLayers[c][i]);
                }

                graph.Layers.Add(merged);
            }

            return new SortResult(dummyNodeCount, layers.Count, type1ConflictCount, hasUnresolvedWidth);
        }

        /// <summary>
        /// グローバルなレイヤー構造から1カラム分を切り出す。
        /// </summary>
        /// <remarks>
        /// レイヤーの添字はグローバルのまま（空の層はそのまま空リストで残す）。
        /// HorizontalCompaction.PlaceBlock が layers[node.Layer] とグローバル層番号で引くので、
        /// 添字を詰めてはいけない。
        /// </remarks>
        static (List<List<ILayeredNode>> layers, Dictionary<NodeId, ILayeredNode> nodes) SplitColumn(
            List<List<ILayeredNode>> layers,
            Dictionary<NodeId, ILayeredNode> layeredNodes,
            int column)
        {
            var columnLayers = new List<List<ILayeredNode>>(layers.Count);
            foreach (var layer in layers)
            {
                var columnLayer = new List<ILayeredNode>();
                foreach (var node in layer)
                {
                    if (node.Column != column) continue;

                    // Indexはカラム内ローカルに振り直す。
                    // LayerOrderAssignerは隣接ノードを持たないノードの重心にIndexをそのまま使うので、
                    // グローバルな添字のまま渡すとそのノードだけカラムの右端へ飛ぶ。
                    node.Index = columnLayer.Count;
                    columnLayer.Add(node);
                }

                columnLayers.Add(columnLayer);
            }

            // HorizontalCompactionは第2引数を全走査して root/x/sink を引くので、カラム専用の辞書が要る。
            // PlaceBlockのsink伝播は列挙順に依存するため、元の辞書の列挙順を保ったままfilterする。
            var columnNodes = new Dictionary<NodeId, ILayeredNode>(layeredNodes.Count);
            foreach (var (id, node) in layeredNodes)
            {
                if (node.Column == column)
                {
                    columnNodes.Add(id, node);
                }
            }

            return (columnLayers, columnNodes);
        }

        /// <summary>
        /// ノードのx座標を計算する
        /// https://arxiv.org/abs/2008.01252 をそのまま実装した
        /// </summary>
        static Dictionary<NodeId, float> HorizontalCompaction(
            List<List<ILayeredNode>> layers,
            Dictionary<NodeId, ILayeredNode> layeredNodes,
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
            void PlaceBlock(ILayeredNode rootNode)
            {
                if (x.TryAdd(rootNode.Id, 0f))
                {
                    var nodeInBlock = rootNode;
                    while (true)
                    {
                        if (nodeInBlock.Index > 0)
                        {
                            // 左にあるノード
                            var pred = layers[nodeInBlock.Layer][nodeInBlock.Index - 1];
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
            List<List<ILayeredNode>> layers,
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


            var temp = new List<ILayeredNode>();
            foreach (var layer in layers)
            {
                var prevParentIndex = -1;
                foreach (var child in layer)
                {
                    var parentCount = child.Parents.Count;
                    if (parentCount == 0) continue;
                    temp.Clear();
                    temp.AddRange(child.Parents.Select(x => x.Node).OrderBy(x => x.Index));

                    // 親の中点が２つある場合は両方のalignを自分にする
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
        static HashSet<(NodeId up, NodeId down)> MarkType1ConflictEdges(List<List<ILayeredNode>> layers)
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
                    ILayeredNode innerParent = null;
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

        (List<List<ILayeredNode>> layers, Dictionary<NodeId, ILayeredNode> nodeSet) ConstructLayers(List<LayeredNode> unsortedNodes)
        {
            // TODO: pool list
            var layers = new List<List<ILayeredNode>>();
            var layeredNodes = new Dictionary<NodeId, ILayeredNode>();

            // step1: 入力を持たないノード（どこにも繋がっていない孤立ノードを含む）を最上段に置く
            // 孤立ノードを専用の層に分けると、孤立ノードのないカラムの1行目が丸ごと空になる
            var sources = new List<ILayeredNode>();
            for (var i = 0; i < unsortedNodes.Count;)
            {
                var node = unsortedNodes[i];
                if (node.EdgesToParent.Count == 0)
                {
                    sources.Add(node);
                    node.Layer = 0;
                    node.Index = sources.Count - 1;
                    layeredNodes.Add(node.Id, node);
                    unsortedNodes.RemoveAt(i);
                }
                else
                {
                    i++;
                }
            }

            // 空でも必ず追加する。Layers[0]が存在する前提のコードがある（LayeredGraph.AddNode）
            layers.Add(sources);

            // step2: それ以外のノードを最長パス法で layer に追加
            while (unsortedNodes.Count > 0)
            {
                var found = false;
                var layerIndex = layers.Count;
                var layer = new List<ILayeredNode>();
                for (var i = 0; i < unsortedNodes.Count;)
                {
                    var node = unsortedNodes[i];
                    var edgesToNode = node.EdgesToParent;
                    // 全ての親がiよりも上のレイヤーなら追加してよい
                    if (edgesToNode.All(e => layeredNodes.TryGetValue(e.EdgeView.Edge.OutputSlot.NodeId, out var parent)
                                             && parent.Layer < layerIndex))
                    {
                        // Debug.Log($"layer:{layerIndex}, {node.Name}");
                        node.Layer = layerIndex;
                        layer.Add(node);
                        node.Index = layer.Count - 1;
                        layeredNodes.Add(node.Id, node);
                        unsortedNodes.RemoveAt(i);

                        // dummy nodeを上から順に追加する
                        foreach (var edge in edgesToNode)
                        {
                            var outputNode = layeredNodes[edge.EdgeView.Edge.OutputSlot.NodeId];

                            // カラムを跨ぐエッジはdummy nodeを作らず、Parents/Childrenにも入れない。
                            // これでレイヤー内の並び替えとx圧縮がカラム内に閉じ、
                            // 跨ぎエッジはスロット同士を結ぶ直線として描かれる。
                            if (outputNode.Column != node.Column) continue;

                            var outputLayerIndex = outputNode.Layer;

                            var parent = outputNode;
                            var parentSlotIndex = edge.EdgeView.Edge.OutputSlot.Index;
                            for (var j = outputLayerIndex + 1; j < layerIndex; j++)
                            {
                                // TODO: pool dummy node
                                var dummyNode = new DummyNode(NodeId.Generate())
                                {
                                    Layer = j,
                                    Column = node.Column
                                };
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

                            node.Parents.Add((parent, parentSlotIndex));
                            parent.Children.Add((node, edge.EdgeView.Edge.InputSlot.Index));
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
    }
}
