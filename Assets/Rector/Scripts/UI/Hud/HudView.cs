using System;
using R3;
using Rector.UI.GraphPages;
using Rector.UI.Graphs;
using UnityEngine;
using UnityEngine.UIElements;

namespace Rector.UI.Hud
{
    public sealed class HudView
    {
        readonly Label timeLabel;
        readonly Label versionLabel;
        readonly Label fpsLabel;
        readonly Label systemMemoryLabel;
        readonly Label totalMemoryLabel;
        readonly Label nodeCountLabel;
        readonly Label edgeCountLabel;
        readonly Label layerCountLabel;
        readonly Label dummyNodeCountLabel;
        readonly Label type1ConflictCountLabel;
        readonly ConsoleView consoleView;
        readonly HudFrameView hudFrameView;

        public GraphPage GraphPage { get; }
        public ButtonListPageView ScenePageView { get; }
        public ButtonListPageView SystemPageView { get; }
        public ButtonListPageView AudioInputDevicePageView { get; }
        public ButtonListPageView DisplaySettingsPageView { get; }
        public CopyrightNoticesPageView CopyrightNoticesPageView { get; }

        public HudView(VisualElement root, UIInputAction uiInputAction, GraphInputAction graphInputAction, NodeTemplateRepository nodeTemplateRepository)
        {
            var footer = root.Q<VisualElement>("footer");
            versionLabel = root.Q<Label>("version-label");
            timeLabel = footer.Q<Label>("time-label");
            fpsLabel = footer.Q<Label>("fps-label");
            systemMemoryLabel = footer.Q<Label>("system-memory-label");
            totalMemoryLabel = footer.Q<Label>("total-memory-label");
            nodeCountLabel = footer.Q<Label>("node-count-label");
            edgeCountLabel = footer.Q<Label>("edge-count-label");
            layerCountLabel = footer.Q<Label>("layer-count-label");
            dummyNodeCountLabel = footer.Q<Label>("dummy-node-count-label");
            type1ConflictCountLabel = footer.Q<Label>("type1-conflict-count-label");
            hudFrameView = new HudFrameView(root);

            consoleView = new ConsoleView(root.Q<VisualElement>("console"));

            GraphPage = new GraphPage(root.Q<VisualElement>("graph-page"), graphInputAction, nodeTemplateRepository);
            ScenePageView = new ButtonListPageView(root.Q<VisualElement>("scene-page"), uiInputAction);
            SystemPageView = new ButtonListPageView(root.Q<VisualElement>("system-page"), uiInputAction);
            AudioInputDevicePageView = new ButtonListPageView(root.Q<VisualElement>("audio-input-device-page"), uiInputAction);
            DisplaySettingsPageView = new ButtonListPageView(root.Q<VisualElement>("display-settings-page"), uiInputAction);
            CopyrightNoticesPageView = new CopyrightNoticesPageView(root.Q<VisualElement>("copyright-notices-page"), uiInputAction);
        }

        public IDisposable Bind(HudModel viewModel)
        {
            versionLabel.text = viewModel.VersionText;
            return new CompositeDisposable(
                viewModel.PlayTime.Subscribe(x => timeLabel.text = ToTimeText(x)),
                viewModel.Fps.Subscribe(x => fpsLabel.text = $"{x:F1}"),
                consoleView.Bind(),
                viewModel.SystemUsedMemory.Subscribe(x => systemMemoryLabel.text = $"{x / (1024f * 1024f):F1}MB"),
                viewModel.TotalUsedMemory.Subscribe(x => totalMemoryLabel.text = $"{x / (1024f * 1024f):F1}MB"),
                viewModel.NodeCount.Subscribe(x => nodeCountLabel.text = $"{x}"),
                viewModel.EdgeCount.Subscribe(x => edgeCountLabel.text = $"{x}"),
                viewModel.LayerCount.Subscribe(x => layerCountLabel.text = $"{x}"),
                viewModel.DummyNodeCount.Subscribe(x => dummyNodeCountLabel.text = $"{x}"),
                viewModel.Type1ConflictCount.Subscribe(x => type1ConflictCountLabel.text = $"{x}"),
                hudFrameView.Bind(viewModel.FrameColor)
            );
        }

        static string ToTimeText(float time)
        {
            var h = Mathf.FloorToInt(time / (60 * 60));
            var min = Mathf.FloorToInt((time / 60) % 60);
            var sec = time % 60;
            return $"{h:00}:{min:00}:{sec:00.00}";
        }
    }
}
