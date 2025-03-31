using System.Collections.Generic;
using System.Linq;
using Lasp;
using R3;
using UnityEngine;
using UnityEngine.Assertions;

namespace Rector.Audio
{
    public sealed class AudioInputDeviceManager
    {
        readonly ReactiveProperty<AudioInputDeviceInfo> currentInputDevice = new(AudioInputDeviceInfo.Empty);
        public ReadOnlyReactiveProperty<AudioInputDeviceInfo> CurrentInputDevice => currentInputDevice;

        readonly Transform audioInputStreamParent;

        const string PrefsKey = "Rector_AudioInputDevice";

        public AudioInputStream InputStream { get; private set; }

        public AudioInputDeviceManager(Transform audioInputStreamParent)
        {
            this.audioInputStreamParent = audioInputStreamParent;
        }

        public IEnumerable<AudioInputDeviceInfo> GetInputDevices()
        {
            return AudioSystem.InputDevices.Select(d => new AudioInputDeviceInfo(d.Name, d.ID));
        }

        public void SwitchDevice(AudioInputDeviceInfo audioInputDeviceInfo)
        {
            Assert.IsTrue(audioInputDeviceInfo.IsValid);
            var deviceDescriptor = AudioSystem.GetInputDevice(audioInputDeviceInfo.Id);
            if (!deviceDescriptor.IsValid)
            {
                Debug.LogError($"Invalid device: {audioInputDeviceInfo.Name}");
                return;
            }

            if (InputStream != null)
            {
                InputStream.Dispose();
                InputStream = null;
            }

            // NOTE: ステレオの場合、Lしか見てないのに注意
            InputStream = AudioInputStream.Create(audioInputDeviceInfo, 0, audioInputStreamParent);
            currentInputDevice.Value = audioInputDeviceInfo;
            PlayerPrefs.SetString(PrefsKey, audioInputDeviceInfo.Id);
            RectorLogger.AudioInputDevice(audioInputDeviceInfo.Id, audioInputDeviceInfo.Name);
        }

        public void Clear()
        {
            if (InputStream != null)
            {
                InputStream.Dispose();
                InputStream = null;
            }

            currentInputDevice.Value = AudioInputDeviceInfo.Empty;
            PlayerPrefs.DeleteKey(PrefsKey);
        }

        public void ReloadLastDevice()
        {
            var lastDeviceId = PlayerPrefs.GetString(PrefsKey, "");
            if (string.IsNullOrEmpty(lastDeviceId)) return;
            try
            {
                var descriptor = AudioSystem.GetInputDevice(lastDeviceId);
                if (descriptor.IsValid)
                {
                    SwitchDevice(new AudioInputDeviceInfo(descriptor.Name, descriptor.ID));
                }
            }
            catch
            {
                PlayerPrefs.DeleteKey(PrefsKey);
                throw;
            }
        }
    }
}
