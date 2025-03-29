using System;
using Rector.UI.Graphs.Nodes;

namespace Rector.UI.Graphs
{
    public sealed class NodeTemplateV2
    {
        public readonly NodeTemplateId Id;
        public readonly string Category;
        public readonly string Name;
        public readonly Func<NodeId, NodeView> Factory;

        NodeTemplateV2(NodeTemplateId id, string category, string name, Func<NodeId, NodeView> factory)
        {
            Id = id;
            Category = category;
            Name = name;
            Factory = factory;
        }

        public static NodeTemplateV2 Create(string category, string name, Func<NodeId, NodeView> factory)
        {
            return new NodeTemplateV2(NodeTemplateId.Generate(), category, name, factory);
        }
    }
}
