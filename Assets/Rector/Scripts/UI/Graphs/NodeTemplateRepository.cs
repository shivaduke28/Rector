using System.Collections.Generic;

namespace Rector.UI.Graphs
{
    public sealed class NodeTemplateRepository
    {
        readonly Dictionary<NodeTemplateId, NodeTemplate> factories = new();
        readonly Dictionary<string, List<NodeTemplate>> categoryNodeSet = new();
        public IReadOnlyDictionary<string, List<NodeTemplate>> CategoryNodeSet => categoryNodeSet;

        public void Add(NodeTemplate factory)
        {
            factories.Add(factory.Id, factory);
            if (!categoryNodeSet.TryGetValue(factory.Category, out var list))
            {
                list = new List<NodeTemplate>();
                categoryNodeSet.Add(factory.Category, list);
            }
            list.Add(factory);
        }

        public IEnumerable<NodeTemplate> GetAll()
        {
            return factories.Values;
        }

        public void Remove(NodeTemplateId id)
        {
            factories.Remove(id);
        }
    }
}
