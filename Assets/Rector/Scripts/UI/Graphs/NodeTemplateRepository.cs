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

        public bool Remove(NodeTemplateId id, out NodeTemplate nodeTemplate)
        {
            if (factories.Remove(id, out nodeTemplate))
            {
                if (categoryNodeSet.TryGetValue(nodeTemplate.Category, out var list))
                {
                    list.Remove(nodeTemplate);
                    if (list.Count == 0)
                    {
                        categoryNodeSet.Remove(nodeTemplate.Category);
                    }
                }

                return true;
            }

            nodeTemplate = null;
            return false;
        }
    }
}
