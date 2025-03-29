using System.Collections.Generic;

namespace Rector.UI.Graphs
{
    public sealed class NodeTemplateRepositoryV2
    {
        readonly Dictionary<NodeTemplateId, NodeTemplateV2> factories = new();
        readonly Dictionary<string, List<NodeTemplateV2>> categoryNodeSet = new();
        public IReadOnlyDictionary<string, List<NodeTemplateV2>> CategoryNodeSet => categoryNodeSet;

        public void Add(NodeTemplateV2 factory)
        {
            factories.Add(factory.Id, factory);
            if (!categoryNodeSet.TryGetValue(factory.Category, out var list))
            {
                list = new List<NodeTemplateV2>();
                categoryNodeSet.Add(factory.Category, list);
            }
            list.Add(factory);
        }

        public IEnumerable<NodeTemplateV2> GetAll()
        {
            return factories.Values;
        }

        public void Remove(NodeTemplateId id)
        {
            factories.Remove(id);
        }
    }
}
