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

        public IDisposable RegisterNodeBehaviour(NodeTemplateRepository repository, GraphPage graphPage)
        {
            foreach (var nodeBehaviour in nodeBehaviours)
            {
                var template = NodeTemplate.Create(nodeBehaviour.Category, nodeBehaviour.name, id => Create(id, nodeBehaviour));
                repository.Add(template);
                registered.Add(template.Id);
            }

            return Disposable.Create(() =>
            {
                var graph = graphPage.Graph;
                foreach (var templateId in registered)
                {
                    if (repository.Remove(templateId, out var nodeTemplate))
                    {
                        foreach (var nodeId in nodeTemplate.NodeIds)
                        {
                            graph.RemoveNode(nodeId);
                        }
                    }
                }
                registered.Clear();

                graphPage.Sort();
            });


            static NodeView Create(NodeId id, NodeBehaviour behaviour)
            {
                var node = new BehaviourNode(id, behaviour);
                var ve = VisualElementFactory.Instance.CreateNode();
                var nodeView = new NodeView(ve, node);
                return nodeView;
            }
        }

        void Reset()
        {
            RetrieveNodeBehaviours();
        }
    }
}
