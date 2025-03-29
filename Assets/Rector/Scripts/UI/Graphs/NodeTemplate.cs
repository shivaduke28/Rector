using System;
using Rector.UI.Graphs.Nodes;

namespace Rector.UI.Graphs
{
    public sealed class NodeTemplate
    {
        public readonly NodeTemplateId Id;
        public readonly string Category;
        public readonly string Name;
        public readonly Func<NodeId, NodeView> Factory;

        NodeTemplate(NodeTemplateId id, string category, string name, Func<NodeId, NodeView> factory)
        {
            Id = id;
            Category = category;
            Name = name;
            Factory = factory;
        }

        public static NodeTemplate Create(string category, string name, Func<NodeId, NodeView> factory)
        {
            return new NodeTemplate(NodeTemplateId.Generate(), category, name, factory);
        }
    }
}
