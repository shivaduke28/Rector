using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using R3;
using Rector.Nodes;
using Rector.UI;
using Rector.UI.Graphs;
using Rector.UI.Graphs.Nodes;
using UnityEngine.SceneManagement;

namespace Rector
{
    public sealed class SceneManager : IDisposable
    {
        readonly SceneSettings sceneSettings;
        readonly LoadingView loadingView;
        readonly NodeTemplateRepository nodeTemplateRepository;
        readonly CancellationTokenSource cts = new();
        readonly ReactiveProperty<string> currentScene = new("");
        public ReadOnlyReactiveProperty<string> CurrentScene => currentScene;

        public string[] GetScenes() => sceneSettings.sceneNames;
        readonly HashSet<NodeTemplateId> registeredNodeTemplates = new();

        public SceneManager(LoadingView loadingView, SceneSettings sceneSettings, NodeTemplateRepository nodeTemplateRepository)
        {
            this.loadingView = loadingView;
            this.sceneSettings = sceneSettings;
            this.nodeTemplateRepository = nodeTemplateRepository;
        }

        public void Load(string sceneName)
        {
            if (currentScene.Value == sceneName) return;
            LoadAsync(sceneName, cts.Token).Forget();
        }

        async UniTaskVoid LoadAsync(string sceneName, CancellationToken token)
        {
            if (!string.IsNullOrEmpty(currentScene.Value))
            {
                UnregisterNodeTemplates();
                var current = currentScene.Value;
                currentScene.Value = "";
                await UnityEngine.SceneManagement.SceneManager.UnloadSceneAsync(current).ToUniTask(cancellationToken: token);
                loadingView.SetActive(true);
            }

            await UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive).ToUniTask(cancellationToken: token);
            currentScene.Value = sceneName;
            // set loaded scene active to apply Skybox and Fog.
            UnityEngine.SceneManagement.SceneManager.SetActiveScene(UnityEngine.SceneManagement.SceneManager.GetSceneByName(sceneName));
            loadingView.SetActive(false);

            RegisterNodeTemplates();
        }

        void UnregisterNodeTemplates()
        {
            foreach (var id in registeredNodeTemplates)
            {
                nodeTemplateRepository.Remove(id);
            }

            registeredNodeTemplates.Clear();
        }

        void RegisterNodeTemplates()
        {
            var rootObjects = UnityEngine.SceneManagement.SceneManager.GetSceneByName(currentScene.Value).GetRootGameObjects();
            foreach (var rootObject in rootObjects)
            {
                var nodeBehaviours = rootObject.GetComponentsInChildren<NodeBehaviour>().OrderBy(b => b.name);
                foreach (var nodeBehaviour in nodeBehaviours)
                {
                    var template = NodeTemplate.Create(NodeCategory.Scene, nodeBehaviour.name, id => Create(new BehaviourNode(id, nodeBehaviour)));
                    nodeTemplateRepository.Add(template);
                    registeredNodeTemplates.Add(template.Id);
                }
            }

            return;

            static NodeView Create(Node node)
            {
                var ve = VisualElementFactory.Instance.CreateNode();
                var nodeView = new NodeView(ve, node);
                return nodeView;
            }
        }

        void IDisposable.Dispose()
        {
            cts.Cancel();
            cts.Dispose();
        }
    }
}
