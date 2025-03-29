using System;
using System.Collections.Generic;
using Rector.UI.Graphs;
using Rector.UI.Graphs.Nodes;
using Rector.UI.Graphs.Slots;
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

        public void AddNode(NodeView nodeView)
        {
            var layeredNode = new LayeredNode(nodeView);
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

        public bool TryGetNode(NodeId id, out LayeredNode node)
        {
            if (nodes.TryGetValue(id, out var n) && n is LayeredNode layeredNode)
            {
                node = layeredNode;
                if (layeredNode.NodeView.Node is IDisposable disposable)
                {
                    disposable.Dispose();
                }

                return true;
            }

            node = null;
            return false;
        }

        public void RemoveNode(NodeId id)
        {
            if (nodes.Remove(id, out var n) && n is LayeredNode layeredNode)
            {
                RemoveEdgesFrom(layeredNode.NodeView.Node);

                Layers[layeredNode.Layer].Remove(layeredNode);
                layeredNode.NodeView.RemoveFrom(nodeRoot);
                layeredNode.NodeView.Dispose();

                RectorLogger.DeleteNode(layeredNode.NodeView.Node);
            }
        }

        /// <remarks>
        /// LayeredNodeのEdgesToChild, EdgesToParentからRemoveするのでforeachの中で呼ぶと例外が出る
        /// </remarks>
        /// <param name="id"></param>
        public bool RemoveEdge(EdgeId id)
        {
            if (Edges.Remove(id, out var layeredEdge))
            {
                edgeRoot.Remove(layeredEdge.EdgeView);
                layeredEdge.EdgeView.Dispose();

                var edge = layeredEdge.EdgeView.Edge;
                if (TryGetNode(edge.OutputSlot.NodeId, out var outputNode) && TryGetNode(edge.InputSlot.NodeId, out var inputNode))
                {
                    RectorLogger.DeleteEdge(edge, outputNode.NodeView.Node, inputNode.NodeView.Node);

                    outputNode.EdgesToChild.Remove(layeredEdge);
                    inputNode.EdgesToParent.Remove(layeredEdge);
                }

                return true;
            }

            return false;
        }

        public void RemoveEdgesFrom(Node selectedNode)
        {
            if (TryGetNode(selectedNode.Id, out var node))
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
    }
}
