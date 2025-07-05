using System;
using System.Collections.Generic;
using System.Linq;
using R3;
using Rector.NodeBehaviours;
using Rector.UI;
using Rector.UI.GraphPages;
using Rector.UI.Graphs;
using Rector.UI.Graphs.Nodes;
using Rector.UI.LayeredGraphDrawing;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Rector
{
    [AddComponentMenu("Rector/BG Scene")]
    public sealed class BGScene : MonoBehaviour
    {
        [SerializeField] NodeBehaviour[] nodeBehaviours;


        readonly HashSet<NodeTemplateId> registered = new();

        public void RetrieveNodeBehaviours()
        {
            var rootObjects = SceneManager.GetActiveScene().GetRootGameObjects();
            nodeBehaviours = rootObjects.SelectMany(go => go.GetComponentsInChildren<NodeBehaviour>()).ToArray();
        }

        public IDisposable RegisterNodeBehaviour(NodeTemplateRepository repository, NodeBehaviourProxyRepository proxyRepository, GraphPage graphPage)
        {
            foreach (var nodeBehaviour in nodeBehaviours)
            {
                var proxy = proxyRepository.GetOrCreateProxy(nodeBehaviour);
                var category = nodeBehaviour.Category;
                var template = NodeTemplate.Create(category, nodeBehaviour.name, id =>
                {
                    var node = new BehaviourNode(id, proxy);
                    var ve = VisualElementFactory.Instance.CreateNode();
                    var nodeView = new NodeView(ve, node);
                    return nodeView;
                });
                repository.Add(template);
                registered.Add(template.Id);
            }

            return Disposable.Create(() =>
            {
                foreach (var templateId in registered)
                {
                    repository.Remove(templateId);
                }

                registered.Clear();

                graphPage.Sort();
            });
        }

        void Reset()
        {
            RetrieveNodeBehaviours();
        }
    }
}
