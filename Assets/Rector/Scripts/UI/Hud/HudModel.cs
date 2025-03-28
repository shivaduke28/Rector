using System;
using R3;
using Rector.UI.Graphs;
using UnityEngine;

namespace Rector.UI.Hud
{
    public sealed class HudModel : IInitializable, IDisposable
    {
        readonly HudView view;
        readonly MemoryStatsRecorder memoryStatsRecorder;

        readonly SystemPageModel systemPageModel;
        readonly GraphPage graphPage;
        readonly ScenePageModel scenePageModel;

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


        public HudModel(HudView view,
            GraphPage graphPage,
            ScenePageModel scenePageModel,
            SystemPageModel systemPageModel,
            MemoryStatsRecorder memoryStatsRecorder)
        {
            this.view = view;
            this.memoryStatsRecorder = memoryStatsRecorder;

            this.graphPage = graphPage;
            this.scenePageModel = scenePageModel;
            this.systemPageModel = systemPageModel;
        }

        public void Initialize()
        {
            view.Bind(this).AddTo(disposable);
            Observable.EveryUpdate(UnityFrameProvider.Update).Subscribe(_ => UpdateFps()).AddTo(disposable);

            graphPage.OpenScenePage.Subscribe(_ => OpenScene()).AddTo(disposable);
            graphPage.OpenSystemPage.Subscribe(_ => OpenSystem()).AddTo(disposable);

            graphPage.Enter();
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

        void OpenSystem()
        {
            graphPage.Exit();
            systemPageModel.Enter(ResumeFromSystem);
        }

        void OpenScene()
        {
            graphPage.Exit();
            scenePageModel.Enter(ResumeFromScene);
        }

        void ResumeFromScene()
        {
            graphPage.Enter();
        }

        void ResumeFromSystem()
        {
            graphPage.Enter();
        }
    }
}
