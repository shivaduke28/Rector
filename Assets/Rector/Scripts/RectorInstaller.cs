using System;
using System.Collections.Generic;
using System.Threading;
using Rector.Audio;
using Rector.Cameras;
using Rector.Cli;
using Rector.Midi;
using Rector.NodeBehaviours;
using Rector.Osc;
using Rector.UI;
using Rector.UI.GraphPages;
using Rector.UI.Graphs;
using Rector.UI.Graphs.Serialization;
using Rector.UI.Hud;
using Rector.Vfx;
using Unity.Cinemachine;
using UnityEngine;

namespace Rector
{
    public sealed class RectorInstaller : MonoBehaviour
    {
        [SerializeField] Camera mainCamera;
        [SerializeField] CinemachineBrain cinemachineBrain;

        [SerializeField] CameraNodeBehaviour[] cameraBehaviours;
        [SerializeField] LoadingView loadingView;
        [SerializeField] RectorSettingsAsset rectorSettingsAsset;
        [SerializeField] RectorUISettingsAsset rectorUISettingsAsset;
        [SerializeField] HudContainer hudContainer;

        RectorInput rectorInput;

        readonly List<IInitializable> initializables = new();
        readonly List<IDisposable> disposables = new();
        readonly CancellationTokenSource cts = new();

        void Start()
        {
            VisualElementFactory.Initialize(rectorUISettingsAsset);

            var audioInputDeviceManager = Register(new AudioInputDeviceManager(transform));

            // input
            rectorInput = Register(new RectorInput());
            rectorInput.Enable();

            // audio
            var beatModel = Register(new BeatModel());
            var mixerModel = Register(new AudioMixerModel(audioInputDeviceManager));

            Register(new ThresholdAdjuster(mixerModel));

            // midi
            var midiInputDeviceManager = Register(new MidiInputDeviceManager());
            var midiModel = Register(new MidiModel(midiInputDeviceManager));

            // osc
            var oscInputSetting = Register(new OscInputSetting());
            var oscModel = Register(new OscModel(oscInputSetting));

            // vfx
            var vfxManager = Register(new VfxManager(rectorSettingsAsset.vfxSettings));

            // camera
            var cameraManager = Register(new CameraManager(cinemachineBrain, cameraBehaviours));

            // node system
            var nodeBehaviourProxyRepository = Register(new NodeBehaviourProxyRepository());
            var nodeTemplateRepository = Register(new NodeTemplateRepository());
            Register(nodeTemplateRepository);

            var uiInputAction = Register(new UIInputAction(rectorInput));
            var graphInputAction = Register(new GraphInputAction(rectorInput));

            var hudRoot = hudContainer.Root;
            var hudView = new HudView(hudRoot, uiInputAction, graphInputAction, nodeTemplateRepository);
            var graphPage = Register(hudView.GraphPage);
            var bgSceneManager = Register(new BGSceneManager(loadingView, rectorSettingsAsset.sceneSettings, nodeTemplateRepository, nodeBehaviourProxyRepository, graphPage));
            var scenePage = Register(new ScenePageModel(hudView.ScenePageView, bgSceneManager));
            var audioInputDevicePage = Register(new AudioInputDevicePageModel(audioInputDeviceManager, hudView.AudioInputDevicePageView));
            var midiInputDevicePage = Register(new MidiInputDevicePageModel(midiInputDeviceManager, hudView.MidiInputDevicePageView));
            var oscSettingsPage = Register(new OscSettingsPageModel(oscInputSetting, oscModel, hudView.OscSettingsPageView));
            var displaySettingsPage = Register(new DisplaySettingsPageModel(hudView.DisplaySettingsPageView));
            var graphSettingsPage = Register(new GraphSettingsPageModel(hudView.GraphSettingsPageView, graphPage.Groups, graphPage.GuideSettings));
            var graphSaveManager = new GraphSaveManager(graphPage, nodeTemplateRepository);
            var confirmDialog = Register(new ConfirmDialogModel(hudView.ConfirmDialogView));
            var presetLoadPage = Register(new PresetLoadPageModel(hudView.PresetLoadPageView, graphSaveManager));
            var presetManagePage = Register(new PresetManagePageModel(hudView.PresetManagePageView, graphSaveManager, confirmDialog));
            var copyrightNoticesPage = Register(new CopyrightNoticesPageModel(hudView.CopyrightNoticesPageView));
            var memoryStatsRecorder = Register(new MemoryStatsRecorder());

            var menuPage = Register(new SystemPageModel(
                audioInputDevicePage,
                midiInputDevicePage,
                oscSettingsPage,
                displaySettingsPage,
                graphSettingsPage,
                presetLoadPage,
                presetManagePage,
                copyrightNoticesPage,
                confirmDialog,
                graphPage,
                hudView.SystemPageView));
            var hudModel = Register(new HudModel(hudView, graphPage, scenePage, menuPage, memoryStatsRecorder));

            Register(new NodeTemplateRegisterer(
                nodeTemplateRepository,
                nodeBehaviourProxyRepository,
                vfxManager,
                beatModel,
                mixerModel,
                midiModel,
                oscModel,
                cameraManager,
                hudModel
            ));

            // Unity CLI (com.unity.pipeline) から観測・操作するための口。
            // [CliCommand] は static しか登録できないので Instance 経由で参照する。
            CliClient.Register(Register(new CliClient(
                graphPage,
                nodeTemplateRepository,
                vfxManager,
                cameraManager,
                bgSceneManager,
                graphSaveManager
            )));

#if !UNITY_EDITOR
            // disable stack trace
            RectorLogger.DisableStackTrace();
#endif

            // initialize
            foreach (var initializable in initializables)
            {
                initializable.Initialize();
            }

            // logger
            disposables.Add(RectorLogger.SubscribeDebugLog());
            RectorLogger.WelcomeMessage();
            RectorLogger.Resolution(Screen.width, Screen.height, Screen.fullScreenMode);

            // reload last device
            audioInputDeviceManager.ReloadLastDevice();
            midiInputDeviceManager.ReloadSelection();

            // ここで初めて OSC のポートが開く。初期化ループの中で開くと、
            // listening も bind 失敗も HUD コンソールが購読する前に流れて消える
            oscInputSetting.Reload();

            // set first camera active
            cameraManager.GetCameraBehaviours()[0].IsActive.Value = true;
            bgSceneManager.Load(rectorSettingsAsset.sceneSettings.sceneNames[0]);
        }

        T Register<T>(T instance)
        {
            if (instance is IDisposable disposable)
            {
                disposables.Add(disposable);
            }

            if (instance is IInitializable initializable)
            {
                initializables.Add(initializable);
            }

            return instance;
        }

        void OnDestroy()
        {
            rectorInput?.Disable();
            rectorInput?.Dispose();

            cts.Cancel();
            cts.Dispose();

            foreach (var disposable in disposables)
            {
                disposable.Dispose();
            }
        }
    }
}
