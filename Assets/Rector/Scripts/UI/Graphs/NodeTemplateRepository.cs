using System.Collections.Generic;
using UnityEngine;

namespace Rector.UI.Graphs
{
    public sealed class NodeTemplateRepository
    {
        readonly Dictionary<NodeTemplateId, NodeTemplate> templates = new();
        readonly Dictionary<NodeCategory, List<NodeTemplate>> categoryNodeSet = new();
        public IReadOnlyDictionary<NodeCategory, List<NodeTemplate>> CategoryNodeSet => categoryNodeSet;

        public void Add(NodeTemplate template)
        {
            // Id は起動を跨いで安定なので、衝突は登録側の間違い(同じクラスを2回、guid の重複)。
            // 作成メニューからは消さず、グラフのロードで取り違える可能性だけ警告する。
            if (!templates.TryAdd(template.Id, template))
            {
                Debug.LogWarning($"Duplicate node template id '{template.Id}' for '{template.Name}'. Saved graphs may restore the wrong node.");
                return;
            }

            if (!categoryNodeSet.TryGetValue(template.Category, out var list))
            {
                list = new List<NodeTemplate>();
                categoryNodeSet.Add(template.Category, list);
            }

            list.Add(template);
        }

        public bool TryGet(NodeTemplateId id, out NodeTemplate template) => templates.TryGetValue(id, out template);

        public IEnumerable<NodeTemplate> GetAll()
        {
            return templates.Values;
        }

        public bool Remove(NodeTemplateId id)
        {
            if (templates.Remove(id, out var nodeTemplate))
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

            return false;
        }
    }
}
