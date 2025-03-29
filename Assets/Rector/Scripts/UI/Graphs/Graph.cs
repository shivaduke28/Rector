using System;
using System.Collections.Generic;
using System.Linq;
using R3;
using Rector.UI.Graphs.Nodes;

namespace Rector.UI.Graphs
{
    public sealed class Graph : IDisposable
    {
        readonly Dictionary<NodeId, Node> nodeSet = new();
        readonly Dictionary<EdgeId, Edge> edgeSet = new();
        public IEnumerable<Node> Nodes => nodeSet.Values;

        public IEnumerable<Edge> Edges => edgeSet.Values;
        readonly Subject<Node> onNodeAdded = new();
        readonly Subject<Node> onNodeRemoved = new();
        public Observable<Node> OnNodeAdded => onNodeAdded;
        public Observable<Node> OnNodeRemoved => onNodeRemoved;

        readonly Subject<Edge> onEdgeAdded = new();
        readonly Subject<EdgeId> onEdgeRemoved = new();
        public Observable<Edge> OnEdgeAdded => onEdgeAdded;
        public Observable<EdgeId> OnEdgeRemoved => onEdgeRemoved;

        public void Add(Node node)
        {
            // Debug.Log($"[AddNode]{node.Id}:{node.Name}");
            if (node is IInitializable initializable)
            {
                initializable.Initialize();
            }
            nodeSet.Add(node.Id, node);
            onNodeAdded.OnNext(node);
        }

        public void AddRange(IEnumerable<Node> nodes)
        {
            foreach (var node in nodes)
            {
                Add(node);
            }
        }

        public void Add(Edge edge)
        {
            // Debug.Log($"[AddEdge]{edge.Id}");
            edgeSet.Add(edge.Id, edge);
            onEdgeAdded.OnNext(edge);
        }

        public bool TryGet(NodeId id, out Node node) => nodeSet.TryGetValue(id, out node);
        public bool TryGet(EdgeId id, out Edge edge) => edgeSet.TryGetValue(id, out edge);

        public void Remove(NodeId id)
        {
            if (nodeSet.Remove(id, out var node))
            {
                var edges = edgeSet.Values.Where(x => x.IsConnectedTo(node)).ToList();
                foreach (var edge in edges)
                {
                    Remove(edge.Id);
                }

                if (node is IDisposable disposable)
                {
                    disposable.Dispose();
                }
                onNodeRemoved.OnNext(node);
            }
        }

        public void Remove(EdgeId id)
        {
            if (edgeSet.Remove(id, out var edge))
            {
                // Debug.Log($"[RemoveEdge]{id}");
                edge.Disconnect();
                onEdgeRemoved.OnNext(id);
            }
        }

        public void Dispose()
        {
            foreach (var node in Nodes)
            {
                if (node is IDisposable disposable)
                {
                    disposable.Dispose();
                }
            }
        }

        public void RemoveEdgesFrom(Node node)
        {
            var edgeIds = new List<EdgeId>();
            foreach (var edge in Edges)
            {
                if (edge.IsConnectedTo(node))
                    edgeIds.Add(edge.Id);
            }
            foreach (var edgeId in edgeIds)
                Remove(edgeId);
        }

        public void RemoveEdgesFrom(ISlot slot)
        {
            var edgeIds = new List<EdgeId>();
            foreach (var edge in Edges)
            {
                if (edge.IsConnectedTo(slot))
                    edgeIds.Add(edge.Id);
            }
            foreach (var edgeId in edgeIds)
            {
                Remove(edgeId);
            }
        }
    }
}
