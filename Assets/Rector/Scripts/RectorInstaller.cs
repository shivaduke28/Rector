﻿using System;
using System.Collections.Generic;
using System.Threading;
using Rector.Audio;
using Rector.Cameras;
using Rector.UI;
using Rector.UI.GraphPages;
using Rector.UI.Graphs;
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

            // vfx
            var vfxManager = Register(new VfxManager(rectorSettingsAsset.vfxSettings));

            // camera
            var cameraManager = Register(new CameraManager(cinemachineBrain, cameraBehaviours));

            var nodeTemplateRepository = Register(new NodeTemplateRepository());
            Register(nodeTemplateRepository);

            var uiInputAction = Register(new UIInputAction(rectorInput));
            var graphInputAction = Register(new GraphInputAction(rectorInput));

            var hudRoot = hudContainer.Root;
            var hudView = new HudView(hudRoot, uiInputAction, graphInputAction, nodeTemplateRepository);
            var graphPage = Register(hudView.GraphPage);
            var bgSceneManager = Register(new BGSceneManager(loadingView, rectorSettingsAsset.sceneSettings, nodeTemplateRepository, graphPage));
            var scenePage = Register(new ScenePageModel(hudView.ScenePageView, bgSceneManager));
            var audioInputDevicePage = Register(new AudioInputDevicePageModel(audioInputDeviceManager, hudView.AudioInputDevicePageView));
            var displaySettingsPage = Register(new DisplaySettingsPageModel(hudView.DisplaySettingsPageView));
            var copyrightNoticesPage = Register(new CopyrightNoticesPageModel(hudView.CopyrightNoticesPageView));
            var memoryStatsRecorder = Register(new MemoryStatsRecorder());

            var menuPage = Register(new SystemPageModel(
                audioInputDevicePage,
                displaySettingsPage,
                copyrightNoticesPage,
                hudView.SystemPageView));
            var hudModel = Register(new HudModel(hudView, graphPage, scenePage, menuPage, memoryStatsRecorder));

            Register(new NodeTemplateRegisterer(
                nodeTemplateRepository,
                vfxManager,
                beatModel,
                mixerModel,
                cameraManager,
                hudModel
            ));

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
