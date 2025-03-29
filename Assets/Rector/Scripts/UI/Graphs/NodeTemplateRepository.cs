using System;
using System.Collections.Generic;
using Rector.UI.Graphs.Nodes;

namespace Rector.UI.Graphs
{
    public sealed class NodeTemplateRepository
    {
        readonly Dictionary<NodeTemplateId, NodeTemplate> templates = new();
        readonly Dictionary<Type, NodeTemplate> typeToTemplate = new();

        public void Add(NodeTemplate template)
        {
            templates.Add(template.Id, template);
            typeToTemplate.TryAdd(template.Type, template);
        }

        public bool TryGet(NodeTemplateId id, out NodeTemplate template)
        {
            return templates.TryGetValue(id, out template);
        }

        public bool TryCreate<T>(out Node node) where T : Node
        {
            if (typeToTemplate.TryGetValue(typeof(T), out var template))
            {
                node = (T)template.Factory(NodeId.Generate());
                return true;
            }

            node = null;
            return false;
        }

        public IEnumerable<NodeTemplate> GetAll()
        {
            return templates.Values;
        }

        public void Remove(NodeTemplateId id)
        {
            if (templates.Remove(id, out var template))
            {
                typeToTemplate.Remove(template.Type);
            }
        }
    }
}
