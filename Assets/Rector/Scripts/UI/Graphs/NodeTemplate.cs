using System;
using Rector.UI.Nodes;

namespace Rector.UI.Graphs
{
    public enum NodeCategory
    {
        Vfx,
        Camera,
        Event,
        Operator,
        Math,
        Scene,
        System,
    }

    public sealed class NodeTemplate
    {
        public readonly NodeTemplateId Id;
        public readonly NodeCategory Category;
        public readonly string Name;
        public readonly Func<NodeId, Node> Factory;
        public readonly Type Type;

        NodeTemplate(NodeTemplateId id, NodeCategory category, string name, Func<NodeId, Node> factory, Type type)
        {
            Id = id;
            Category = category;
            Name = name;
            Factory = factory;
            Type = type;
        }

        public static NodeTemplate Create<T>(NodeCategory category, string name, Func<NodeId, T> factory) where T : Node
        {
            return new NodeTemplate(NodeTemplateId.Generate(), category, name, factory, typeof(T));
        }
    }
}
