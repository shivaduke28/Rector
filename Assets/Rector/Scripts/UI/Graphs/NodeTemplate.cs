using System;
using System.Collections.Generic;
using Rector.UI.Graphs.Nodes;

namespace Rector.UI.Graphs
{
    public sealed class NodeTemplate
    {
        public readonly NodeTemplateId Id;
        public readonly NodeCategory Category;
        public readonly string Name;
        readonly Func<NodeId, NodeView> factory;

        public NodeView Create(NodeId id)
        {
            return factory(id);
        }

        NodeTemplate(NodeTemplateId id, NodeCategory category, string name, Func<NodeId, NodeView> factory)
        {
            Id = id;
            Category = category;
            Name = name;
            this.factory = factory;
        }

        public static NodeTemplate Create(NodeCategory category, string name, Func<NodeId, NodeView> factory)
        {
            return new NodeTemplate(NodeTemplateId.Generate(), category, name, factory);
        }
    }
}
