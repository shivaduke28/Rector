using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using R3;
using Rector.UI.GraphPages;
using Rector.UI.Graphs;
using Rector.UI.LayeredGraphDrawing;
using UnityEngine.SceneManagement;

namespace Rector
{
    public sealed class BGSceneManager : IDisposable
    {
        readonly SceneSettings sceneSettings;
        readonly LoadingView loadingView;
        readonly NodeTemplateRepository nodeTemplateRepository;
        readonly GraphPage graphPage;
        readonly CancellationTokenSource cts = new();
        readonly ReactiveProperty<string> currentScene = new("");
        public ReadOnlyReactiveProperty<string> CurrentScene => currentScene;

        readonly CompositeDisposable bgDisposables = new();

        public string[] GetScenes() => sceneSettings.sceneNames;

        public BGSceneManager(LoadingView loadingView,
            SceneSettings sceneSettings,
            NodeTemplateRepository nodeTemplateRepository,
            GraphPage graphPage)
        {
            this.loadingView = loadingView;
            this.sceneSettings = sceneSettings;
            this.nodeTemplateRepository = nodeTemplateRepository;
            this.graphPage = graphPage;
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
                var current = currentScene.Value;
                currentScene.Value = "";
                bgDisposables.Clear();
                await SceneManager.UnloadSceneAsync(current).ToUniTask(cancellationToken: token);
                loadingView.SetActive(true);
            }

            await SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive).ToUniTask(cancellationToken: token);
            currentScene.Value = sceneName;
            // set loaded scene active to apply Skybox and Fog.
            SceneManager.SetActiveScene(SceneManager.GetSceneByName(sceneName));
            loadingView.SetActive(false);

            RegisterNodeTemplates();
        }

        void RegisterNodeTemplates()
        {
            var rootObjects = SceneManager.GetSceneByName(currentScene.Value).GetRootGameObjects();
            foreach (var rootObject in rootObjects)
            {
                var bgScenes = rootObject.GetComponentsInChildren<BGScene>();
                foreach (var bgScene in bgScenes)
                {
                    bgScene.RegisterNodeBehaviour(nodeTemplateRepository, graphPage).AddTo(bgDisposables);
                }
            }
        }

        void IDisposable.Dispose()
        {
            cts.Cancel();
            cts.Dispose();
        }
    }
}
