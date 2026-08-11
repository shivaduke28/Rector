using System;
using System.Collections.Generic;
using System.Linq;
using R3;
using Rector.UI.GraphPages;
using UnityEngine.InputSystem;

namespace Rector.UI.Hud
{
    /// <summary>
    /// HUD の親メニュー。設定・プリセット・グラフ・アプリの4つに見出しで区切る。
    /// </summary>
    /// <remarks>
    /// 見出しはカーソルが止まらないので、上下は今までどおり項目の間を素通しで動く。
    /// 設定の行から "Settings" を落としているのは見出しと重なるため。グラフの設定だけ
    /// "Graph Editor" にしてあるのは、グラフの中身を触る GRAPH の見出しと読み違えないように。
    /// </remarks>
    public sealed class SystemPageModel : IInitializable, IDisposable, IButtonListPageModel
    {
        readonly ButtonListItem[] items;
        readonly RectorButtonState[] buttons;
        readonly ReactiveProperty<bool> isVisible = new(false);
        int index;

        readonly AudioInputDevicePageModel audioInputDevicePageModel;
        readonly MidiInputDevicePageModel midiInputDevicePageModel;
        readonly OscSettingsPageModel oscSettingsPageModel;
        readonly DisplaySettingsPageModel displaySettingsPageModel;
        readonly GraphSettingsPageModel graphSettingsPageModel;
        readonly PresetLoadPageModel presetLoadPageModel;
        readonly PresetManagePageModel presetManagePageModel;
        readonly CopyrightNoticesPageModel copyrightNoticesPageModel;
        readonly ConfirmDialogModel confirmDialog;
        readonly GraphPage graphPage;
        readonly ButtonListPageView view;
        Action onExit;
        IDisposable disposable;

        public SystemPageModel(
            AudioInputDevicePageModel audioInputDevicePageModel,
            MidiInputDevicePageModel midiInputDevicePageModel,
            OscSettingsPageModel oscSettingsPageModel,
            DisplaySettingsPageModel displaySettingsPageModel,
            GraphSettingsPageModel graphSettingsPageModel,
            PresetLoadPageModel presetLoadPageModel,
            PresetManagePageModel presetManagePageModel,
            CopyrightNoticesPageModel copyrightNoticesPageModel,
            ConfirmDialogModel confirmDialog,
            GraphPage graphPage,
            ButtonListPageView view
        )
        {
            this.audioInputDevicePageModel = audioInputDevicePageModel;
            this.midiInputDevicePageModel = midiInputDevicePageModel;
            this.oscSettingsPageModel = oscSettingsPageModel;
            this.displaySettingsPageModel = displaySettingsPageModel;
            this.graphSettingsPageModel = graphSettingsPageModel;
            this.presetLoadPageModel = presetLoadPageModel;
            this.presetManagePageModel = presetManagePageModel;
            this.copyrightNoticesPageModel = copyrightNoticesPageModel;
            this.confirmDialog = confirmDialog;
            this.graphPage = graphPage;
            this.view = view;

            // 並びの正はこの配列だけ。カーソルの対象はここから見出しを抜いて作る
            items = new[]
            {
                ButtonListItem.Header("SETTINGS"),
                ButtonListItem.Of(new RectorButtonState("Audio", ShowAudioSettings)),
                ButtonListItem.Of(new RectorButtonState("MIDI", ShowMidiSettings)),
                ButtonListItem.Of(new RectorButtonState("OSC", ShowOscSettings)),
                ButtonListItem.Of(new RectorButtonState("Display", ShowDisplaySettings)),
                ButtonListItem.Of(new RectorButtonState("Graph Editor", ShowGraphSettings)),
                ButtonListItem.Header("PRESET"),
                ButtonListItem.Of(new RectorButtonState("Load Preset", ShowLoadPreset)),
                ButtonListItem.Of(new RectorButtonState("Manage Presets", ShowManagePresets)),
                ButtonListItem.Header("GRAPH"),
                ButtonListItem.Of(new RectorButtonState("Clear Graph", ClearGraph)),
                // ページの題が SYSTEM なので、最後のまとまりは APP と呼ぶ
                ButtonListItem.Header("APP"),
                ButtonListItem.Of(new RectorButtonState("Copyright Notices", ShowCopyrightNotices)),
                ButtonListItem.Of(new RectorButtonState("Exit", ExitApplication)),
            };

            buttons = items.Where(x => !x.IsHeader).Select(x => x.Button).ToArray();
        }

        void IInitializable.Initialize()
        {
            disposable = view.Bind(this);
        }

        void IDisposable.Dispose() => disposable?.Dispose();

        public void Enter(Action onExitAction)
        {
            // ALT/Shift は macOS では差分イベント(flagsChanged)でしか届かず、フォーカスが
            // 切れている間に取りこぼすと押下状態が反転したまま自己回復しない。
            // ここでOSの実状態を再送させて矯正する。コストはキー1回押した分と同じ。
            if (Keyboard.current is { } keyboard)
            {
                InputSystem.TrySyncDevice(keyboard);
            }

            onExit = onExitAction;
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

        /// <summary>グラフを全部消す。undoが無いので確認を挟む。</summary>
        void ClearGraph()
        {
            var nodeCount = graphPage.Graph.NodeCount;

            // 空のグラフは消すものが無いので確認しない
            if (nodeCount == 0)
            {
                DoClearGraph();
                return;
            }

            isVisible.Value = false;
            confirmDialog.Enter(
                $"Clear the current graph?   {nodeCount} nodes / {graphPage.Graph.EdgeCount} edges",
                new[] { new ConfirmChoice("Clear", DoClearGraph) },
                Resume);
        }

        void DoClearGraph()
        {
            var nodeCount = graphPage.Graph.NodeCount;
            graphPage.ClearGraph();
            RectorLogger.GraphCleared(nodeCount);
        }

        IEnumerable<ButtonListItem> IButtonListPageModel.GetItems() => items;

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

        void ShowLoadPreset()
        {
            isVisible.Value = false;
            presetLoadPageModel.Enter(Resume);
        }

        void ShowManagePresets()
        {
            isVisible.Value = false;
            presetManagePageModel.Enter(Resume);
        }

        void ShowCopyrightNotices()
        {
            isVisible.Value = false;
            copyrightNoticesPageModel.Enter(Resume);
        }

        void ExitApplication()
        {
            isVisible.Value = false;
            confirmDialog.Enter("Quit Rector?", new[] { new ConfirmChoice("Quit", Quit) }, Resume);
        }

        static void Quit()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.ExitPlaymode();
#else
            UnityEngine.Application.Quit();
#endif
        }
    }
}
