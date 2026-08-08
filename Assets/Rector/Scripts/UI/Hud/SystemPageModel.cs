using System;
using System.Collections.Generic;
using R3;
using Rector.UI.GraphPages;

namespace Rector.UI.Hud
{
    public sealed class SystemPageModel : IInitializable, IDisposable, IButtonListPageModel
    {
        readonly RectorButtonState[] buttons;
        readonly ReactiveProperty<bool> isVisible = new(false);
        int index;

        readonly AudioInputDevicePageModel audioInputDevicePageModel;
        readonly MidiInputDevicePageModel midiInputDevicePageModel;
        readonly OscSettingsPageModel oscSettingsPageModel;
        readonly DisplaySettingsPageModel displaySettingsPageModel;
        readonly GraphSettingsPageModel graphSettingsPageModel;
        readonly GraphSlotPageModel graphSlotPageModel;
        readonly CopyrightNoticesPageModel copyrightNoticesPageModel;
        readonly GraphPage graphPage;
        readonly ButtonListPageView view;
        Action onExit;
        IDisposable disposable;

        const string ClearGraphLabel = "Clear Graph";

        /// <summary>グラフ全消しの確認待ち。ラベルを戻すためにボタンを持っておく。</summary>
        readonly RectorButtonState clearGraphButton;

        bool clearGraphArmed;

        public SystemPageModel(
            AudioInputDevicePageModel audioInputDevicePageModel,
            MidiInputDevicePageModel midiInputDevicePageModel,
            OscSettingsPageModel oscSettingsPageModel,
            DisplaySettingsPageModel displaySettingsPageModel,
            GraphSettingsPageModel graphSettingsPageModel,
            GraphSlotPageModel graphSlotPageModel,
            CopyrightNoticesPageModel copyrightNoticesPageModel,
            GraphPage graphPage,
            ButtonListPageView view
        )
        {
            this.audioInputDevicePageModel = audioInputDevicePageModel;
            this.midiInputDevicePageModel = midiInputDevicePageModel;
            this.oscSettingsPageModel = oscSettingsPageModel;
            this.displaySettingsPageModel = displaySettingsPageModel;
            this.graphSettingsPageModel = graphSettingsPageModel;
            this.graphSlotPageModel = graphSlotPageModel;
            this.copyrightNoticesPageModel = copyrightNoticesPageModel;
            this.graphPage = graphPage;
            this.view = view;
            clearGraphButton = new RectorButtonState(ClearGraphLabel, ClearGraph);
            buttons = new[]
            {
                new RectorButtonState("Audio Settings", ShowAudioSettings),
                new RectorButtonState("MIDI Settings", ShowMidiSettings),
                new RectorButtonState("OSC Settings", ShowOscSettings),
                new RectorButtonState("Display Settings", ShowDisplaySettings),
                new RectorButtonState("Graph Settings", ShowGraphSettings),
                new RectorButtonState("Save Graph", ShowSaveGraph),
                new RectorButtonState("Load Graph", ShowLoadGraph),
                clearGraphButton,
                new RectorButtonState("Copyright Notices", ShowCopyrightNotices),
                new RectorButtonState("Exit", ExitApplication),
            };
        }

        void IInitializable.Initialize()
        {
            disposable = view.Bind(this);
        }

        void IDisposable.Dispose() => disposable?.Dispose();

        public void Enter(Action onExitAction)
        {
            onExit = onExitAction;
            DisarmClearGraph();
            isVisible.Value = true;
            // 閉じずに Enter された場合に前のフォーカスが残らないよう、先に外してから先頭へ戻す
            buttons[index].IsFocused.Value = false;
            index = 0;
            buttons[index].IsFocused.Value = true;
        }

        void Resume()
        {
            isVisible.Value = true;
        }

        /// <summary>グラフを全部消す。undoが無いので、もう一度押させて意思を確かめる。</summary>
        void ClearGraph()
        {
            var nodeCount = graphPage.Graph.NodeCount;

            // 空のグラフは消すものが無いので確認しない
            if (nodeCount > 0 && !clearGraphArmed)
            {
                clearGraphArmed = true;
                clearGraphButton.Text.Value = "Clear Graph?   press again to confirm";
                return;
            }

            DisarmClearGraph();
            graphPage.ClearGraph();
            RectorLogger.GraphCleared(nodeCount);
        }

        void DisarmClearGraph()
        {
            clearGraphArmed = false;
            clearGraphButton.Text.Value = ClearGraphLabel;
        }


        IEnumerable<RectorButtonState> IButtonListPageModel.GetButtons() => buttons;

        ReadOnlyReactiveProperty<bool> IButtonListPageModel.IsVisible => isVisible;

        void IButtonListPageModel.Submit() => buttons[index].OnClick();

        void IButtonListPageModel.Cancel()
        {
            buttons[index].IsFocused.Value = false;
            isVisible.Value = false;
            onExit?.Invoke();
        }

        void IButtonListPageModel.Navigate(bool next)
        {
            // 行を移ったら確認は無かったことにする
            DisarmClearGraph();

            buttons[index].IsFocused.Value = false;
            index += next ? 1 : -1;
            index = (index + buttons.Length) % buttons.Length;

            buttons[index].IsFocused.Value = true;
        }


        void ShowAudioSettings()
        {
            isVisible.Value = false;
            audioInputDevicePageModel.Enter(Resume);
        }

        void ShowMidiSettings()
        {
            isVisible.Value = false;
            midiInputDevicePageModel.Enter(Resume);
        }

        void ShowOscSettings()
        {
            isVisible.Value = false;
            oscSettingsPageModel.Enter(Resume);
        }

        void ShowDisplaySettings()
        {
            isVisible.Value = false;
            displaySettingsPageModel.Enter(Resume);
        }

        void ShowGraphSettings()
        {
            isVisible.Value = false;
            graphSettingsPageModel.Enter(Resume);
        }

        void ShowSaveGraph()
        {
            isVisible.Value = false;
            graphSlotPageModel.Enter(GraphSlotPageMode.Save, Resume);
        }

        void ShowLoadGraph()
        {
            isVisible.Value = false;
            graphSlotPageModel.Enter(GraphSlotPageMode.Load, Resume);
        }

        void ShowCopyrightNotices()
        {
            isVisible.Value = false;
            copyrightNoticesPageModel.Enter(Resume);
        }

        void ExitApplication()
        {
            // 確認は挟まない。SystemページはSELECT長押しでしか開かないので誤爆しにくい
#if UNITY_EDITOR
            UnityEditor.EditorApplication.ExitPlaymode();
#else
            UnityEngine.Application.Quit();
#endif
        }
    }
}
