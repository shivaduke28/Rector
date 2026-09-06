using System;
using System.Collections.Generic;
using Rector.UI.Graphs;
using Rector.UI.Graphs.Nodes;
using Rector.UI.Graphs.Slots;
using UnityEngine;
using UnityEngine.UIElements;

namespace Rector.UI.LayeredGraphDrawing
{
    public sealed class LayeredGraph
    {
        public readonly List<List<ILayeredNode>> Layers = new();
        readonly Dictionary<NodeId, ILayeredNode> nodes = new();
        public readonly Dictionary<EdgeId, LayeredEdge> Edges = new();
        readonly List<LayeredEdge> tempEdges = new();

        public int NodeCount => nodes.Count;
        public int EdgeCount => Edges.Count;

        readonly VisualElement nodeRoot;
        readonly VisualElement edgeRoot;

        public LayeredGraph(VisualElement nodeRoot, VisualElement edgeRoot)
        {
            this.nodeRoot = nodeRoot;
            this.edgeRoot = edgeRoot;
            Layers.Add(new List<ILayeredNode>());
        }

        public void AddNode(NodeView nodeView, int group)
        {
            var layeredNode = new LayeredNode(nodeView) { Group = group };
            if (nodes.TryAdd(layeredNode.Id, layeredNode))
            {
                var layer = Layers[0];
                layer.Add(layeredNode);
                layeredNode.Layer = 0;
                layeredNode.Index = layer.Count - 1;

                nodeView.AddTo(nodeRoot);

                if (nodeView.Node is IInitializable initializable)
                {
                    initializable.Initialize();
                }

                RectorLogger.CreateNode(nodeView.Node);
            }
        }

        public void AddEdge(Edge edge)
        {
            if (TryGetNode(edge.OutputSlot.NodeId, out var outputNode) && TryGetNode(edge.InputSlot.NodeId, out var inputNode))
            {
                var edgeView = new EdgeView(outputNode.NodeView.OutputSlotViews[edge.OutputSlot.Index], inputNode.NodeView.InputSlotViews[edge.InputSlot.Index], edge);
                var layeredEdge = new LayeredEdge(edgeView);
                if (Edges.TryAdd(layeredEdge.Id, layeredEdge))
                {
                    edgeView.Repaint();
                    edgeRoot.Add(edgeView);
                    outputNode.EdgesToChild.Add(layeredEdge);
                    inputNode.EdgesToParent.Add(layeredEdge);
                    RectorLogger.CreateEdge(edge, outputNode.NodeView.Node, inputNode.NodeView.Node);
                }
            }
        }

        /// <summary>
        /// グラフ上の全ノード。Layers を歩くのと違い、Sort の前後や DummyNode に左右されない。
        /// </summary>
        public IEnumerable<LayeredNode> Nodes
        {
            get
            {
                foreach (var node in nodes.Values)
                {
                    if (node is LayeredNode layeredNode) yield return layeredNode;
                }
            }
        }

        /// <summary>
        /// 全ノードとエッジを消す。グラフのロードで作り直す前に呼ぶ。
        /// </summary>
        /// <remarks>
        /// RemoveNode が nodes と Layers を書き換えるので、先に id を控えてから回す。
        /// 選択やターゲットの後始末はしないので、GraphPage 側で先に外しておくこと。
        /// </remarks>
        public void ClearNodes()
        {
            var ids = new List<NodeId>(nodes.Count);
            foreach (var id in nodes.Keys)
            {
                ids.Add(id);
            }

            foreach (var id in ids)
            {
                RemoveNode(id);
            }
        }

        public bool TryGetNode(NodeId id, out LayeredNode node)
        {
            if (nodes.TryGetValue(id, out var n) && n is LayeredNode layeredNode)
            {
                node = layeredNode;
                return true;
            }

            node = null;
            return false;
        }

        public void RemoveNode(NodeId id)
        {
            if (nodes.TryGetValue(id, out var n) && n is LayeredNode layeredNode)
            {
                RemoveEdgesFrom(layeredNode);

                // NOTE: remove from nodes **after** removing edges
                nodes.Remove(id);
                Layers[layeredNode.Layer].Remove(layeredNode);
                layeredNode.NodeView.RemoveFrom(nodeRoot);
                layeredNode.NodeView.Dispose();

                if (layeredNode.NodeView.Node is IDisposable disposable)
                {
                    disposable.Dispose();
                }

                RectorLogger.DeleteNode(layeredNode.NodeView.Node);
            }
        }

        /// <remarks>
        /// Nodeを削除するときは先にこれを呼ぶこと
        /// LayeredNodeのEdgesToChild, EdgesToParentからRemoveするのでforeachの中で呼ぶと例外が出る
        /// ILayeredNode.Parent/Childrenを編集しないことに注意
        /// </remarks>
        public bool RemoveEdge(EdgeId id)
        {
            if (Edges.Remove(id, out var layeredEdge))
            {
                edgeRoot.Remove(layeredEdge.EdgeView);
                layeredEdge.EdgeView.Dispose();

                var edge = layeredEdge.EdgeView.Edge;
                edge.Dispose();
                if (TryGetNode(edge.OutputSlot.NodeId, out var outputNode) && TryGetNode(edge.InputSlot.NodeId, out var inputNode))
                {
                    RectorLogger.DeleteEdge(edge, outputNode.NodeView.Node, inputNode.NodeView.Node);

                    outputNode.EdgesToChild.Remove(layeredEdge);
                    inputNode.EdgesToParent.Remove(layeredEdge);
                }
                else
                {
                    Debug.LogError("Nodes not found when removing edge.");
                }

                return true;
            }

            return false;
        }

        public void RemoveEdgesFrom(LayeredNode node)
        {
            {
                tempEdges.Clear();
                tempEdges.AddRange(node.EdgesToParent);
                tempEdges.AddRange(node.EdgesToChild);

                foreach (var edge in tempEdges)
                {
                    RemoveEdge(edge.Id);
                }
            }
        }


        public void RemoveEdgesFrom(ISlot slot)
        {
            if (!TryGetNode(slot.NodeId, out var node)) return;

            tempEdges.Clear();
            switch (slot)
            {
                case OutputSlot outputSlot:
                    {
                        tempEdges.AddRange(node.EdgesToChild);
                        foreach (var edge in tempEdges)
                        {
                            if (edge.Id.OutputSlotIndex == outputSlot.Index)
                            {
                                RemoveEdge(edge.Id);
                            }
                        }

                        break;
                    }
                case InputSlot inputSlot:
                    {
                        tempEdges.AddRange(node.EdgesToParent);
                        foreach (var edge in tempEdges)
                        {
                            if (edge.Id.InputSlotIndex == inputSlot.Index)
                            {
                                RemoveEdge(edge.Id);
                            }
                        }

                        break;
                    }
            }
        }

        public bool ValidateLoop(OutputSlot output, InputSlot input)
        {
            if (output.NodeId.Equals(input.NodeId)) return false;

            var outputNode = output.NodeId;
            var inputNode = input.NodeId;

            if (TryGetNode(outputNode, out var layeredOutputNode) && TryGetNode(inputNode, out var layeredInputNode))
            {
                // input→outputにパスがある場合はloopになるので弾く
                if (CheckRecursively(layeredInputNode, layeredOutputNode))
                {
                    return false;
                }

                return true;
            }
            else
            {
                Debug.LogError("Nodes not found when validating loop.");
                return false;
            }
        }

        /// <summary>
        /// rootから下流にたどれるノードをすべてresultへ足す。root自身は含めない。
        /// </summary>
        /// <remarks>
        /// 菱形(同じノードへ2本の道がある)で二重に積まないよう、通ったノードはresultで判定する。
        /// ループはValidateLoopが弾いているが、万一あってもここで止まる。
        /// </remarks>
        public void CollectDescendants(LayeredNode root, HashSet<LayeredNode> result)
        {
            var visited = new HashSet<LayeredNode> { root };
            var stack = new Stack<LayeredNode>();
            stack.Push(root);

            while (stack.Count > 0)
            {
                var node = stack.Pop();
                foreach (var edge in node.EdgesToChild)
                {
                    if (!TryGetNode(edge.EdgeView.Edge.InputSlot.NodeId, out var child)) continue;
                    if (!visited.Add(child)) continue;

                    result.Add(child);
                    stack.Push(child);
                }
            }
        }

        // fromからtoに向かうedgeがあるかどうかを再帰的に調べる
        bool CheckRecursively(LayeredNode from, LayeredNode to)
        {
            if (from == to) return true;

            foreach (var edge in from.EdgesToChild)
            {
                var childId = edge.EdgeView.Edge.InputSlot.NodeId;
                if (childId == to.Id) return true;

                if (TryGetNode(childId, out var childNode) && CheckRecursively(childNode, to))
                {
                    return true;
                }
            }

            return false;
        }
    }
}
