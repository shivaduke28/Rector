using System;
using R3;
using Rector.Cameras;
using Rector.UI.Graphs;
using UnityEngine;

namespace Rector.UI.Hud
{
    public sealed class HudModel : IInitializable, IDisposable
    {
        enum State
        {
            Graph,
            Scene,
            System
        }

        State state = State.Graph;

        readonly UIInput uiInput;
        readonly HudView view;
        readonly SceneManager sceneManager;
        readonly CameraManager cameraManager;
        readonly MemoryStatsRecorder memoryStatsRecorder;

        readonly SystemPage systemPage;
        readonly GraphPage graphPage;
        readonly ScenePage scenePage;

        // public Observable<string> SceneText => sceneManager.CurrentScene.Select(x => $"Scene: {x}");
        // public Observable<string> CameraText => cameraManager.CurrentCamera.Select(x => $"Camera: {x}");
        public ReadOnlyReactiveProperty<float> SystemUsedMemory => memoryStatsRecorder.SystemUsedMemory;
        public ReadOnlyReactiveProperty<float> TotalUsedMemory => memoryStatsRecorder.TotalUsedMemory;
        public Observable<float> PlayTime => Observable.EveryUpdate(UnityFrameProvider.Update).Select(_ => Time.realtimeSinceStartup);

        public ReadOnlyReactiveProperty<int> NodeCount => graphPage.NodeCount;
        public ReadOnlyReactiveProperty<int> EdgeCount => graphPage.EdgeCount;
        public ReadOnlyReactiveProperty<int> LayerCount => graphPage.LayerCount;
        public ReadOnlyReactiveProperty<int> DummyNodeCount => graphPage.DummyNodeCount;
        public ReadOnlyReactiveProperty<int> Type1ConflictCount => graphPage.Type1ConflictCount;
        public readonly ReactiveProperty<Color> FrameColor = new(new Color(0, 0, 0, 0));

        public readonly ReactiveProperty<float> Fps = new(0);

        float dtAccum;
        int dtCount;
        float fpsUpdateTime;

        public readonly string VersionText = $"{Application.productName} ver.{Application.version}";

        readonly CompositeDisposable disposable = new();


        public HudModel(
            UIInput uiInput,
            HudView view,
            GraphPage graphPage,
            ScenePage scenePage,
            SystemPage systemPage,
            SceneManager sceneManager,
            CameraManager cameraManager, MemoryStatsRecorder memoryStatsRecorder)
        {
            this.uiInput = uiInput;
            this.view = view;
            this.sceneManager = sceneManager;
            this.cameraManager = cameraManager;
            this.memoryStatsRecorder = memoryStatsRecorder;

            this.graphPage = graphPage;
            this.scenePage = scenePage;
            this.systemPage = systemPage;
        }

        public void Initialize()
        {
            view.Bind(this, uiInput).AddTo(disposable);
            Observable.EveryUpdate(UnityFrameProvider.Update).Subscribe(_ => UpdateFps()).AddTo(disposable);
            graphPage.Enter();
            state = State.Graph;
        }

        public void Dispose()
        {
            disposable.Dispose();
        }

        void UpdateFps()
        {
            var deltaTime = Time.deltaTime;
            dtAccum += deltaTime;
            dtCount++;

            if (Time.realtimeSinceStartup - fpsUpdateTime > 0.1f)
            {
                var deltaTimeAvg = dtAccum / dtCount;
                Fps.Value = 1 / deltaTimeAvg;
                dtAccum = 0;
                dtCount = 0;
                fpsUpdateTime = Time.realtimeSinceStartup;
            }
        }

        public void OpenSystem()
        {
            if (state == State.Graph && graphPage.State.Value == GraphPageState.NodeSelection)
            {
                graphPage.Exit();
                systemPage.Enter(ResumeFromSystem);
                state = State.System;
            }
        }

        public void OpenScene()
        {
            if (state == State.Graph && graphPage.State.Value == GraphPageState.NodeSelection)
            {
                graphPage.Exit();
                scenePage.Enter(ResumeFromScene);
                state = State.Scene;
            }
        }

        void ResumeFromScene()
        {
            graphPage.Enter();
            state = State.Graph;
        }

        void ResumeFromSystem()
        {
            graphPage.Enter();
            state = State.Graph;
        }
    }
}
