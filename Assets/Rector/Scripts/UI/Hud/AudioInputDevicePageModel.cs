using System;
using System.Collections.Generic;
using System.Linq;
using R3;
using Rector.Audio;
using Rector.UI.Settings;

namespace Rector.UI.Hud
{
    public sealed class AudioInputDevicePageModel : IInitializable, IDisposable, ISettingsPageModel
    {
        readonly ReactiveProperty<bool> isVisible = new(false);
        readonly AudioInputDeviceManager audioInputDeviceManager;
        readonly SettingsPageView view;

        // 1つだけ選ぶ設定なので、送るたびに入力が切り替わらないメニュー行にする
        readonly SelectorRowState deviceRow;
        readonly ISettingRow[] rows;

        /// <summary>行の候補と対で持つ。確定したインデックスから実体を引く。</summary>
        readonly List<AudioInputDeviceInfo> devices = new();

        readonly CompositeDisposable disposable = new();
        Action onExit;

        ReadOnlyReactiveProperty<bool> ISettingsPageModel.IsVisible => isVisible;
        IReadOnlyList<ISettingRow> ISettingsPageModel.GetRows() => rows;

        public AudioInputDevicePageModel(AudioInputDeviceManager audioInputDeviceManager,
            SettingsPageView view)
        {
            this.audioInputDeviceManager = audioInputDeviceManager;
            this.view = view;

            deviceRow = new SelectorRowState("Input Device", Select);
            rows = new ISettingRow[] { deviceRow };
        }

        public void Enter(Action onExitAction)
        {
            RefreshDevices();

            onExit = onExitAction;
            isVisible.Value = true;
        }

        void IInitializable.Initialize()
        {
            view.Bind(this).AddTo(disposable);
        }

        void RefreshDevices()
        {
            devices.Clear();
            devices.AddRange(audioInputDeviceManager.GetInputDevices().OrderBy(x => x.Name));

            // 入力デバイスが無いのは起こり得る。空のページだと壊れたのと区別が付かない
            if (devices.Count == 0)
            {
                deviceRow.SetOptions(new[] { "None" }, 0);
                return;
            }

            var current = audioInputDeviceManager.CurrentInputDevice.CurrentValue;
            deviceRow.SetOptions(
                devices.Select(x => x.Name).ToArray(),
                current.IsValid ? devices.IndexOf(current) : -1);
        }

        void Select(int index)
        {
            if (index < 0 || index >= devices.Count) return;

            audioInputDeviceManager.SwitchDevice(devices[index]);
        }

        void ISettingsPageModel.Cancel()
        {
            isVisible.Value = false;
            onExit?.Invoke();
            onExit = null;
        }

        void IDisposable.Dispose() => disposable.Dispose();
    }
}
