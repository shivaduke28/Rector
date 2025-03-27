using System;
using System.Collections.Generic;
using System.Linq;
using Lasp;
using R3;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Assertions;

namespace Rector.Audio
{
    public sealed class AudioInputDeviceManager
    {
        readonly ReactiveProperty<AudioInputDeviceInfo> currentInputDevice = new(AudioInputDeviceInfo.Empty);
        public ReadOnlyReactiveProperty<AudioInputDeviceInfo> CurrentInputDevice => currentInputDevice;

        const string PrefsKey = "Rector_AudioInputDevice";

        InputStream inputStream;

        public IEnumerable<AudioInputDeviceInfo> GetInputDevices()
        {
            return AudioSystem.InputDevices.Select(d => new AudioInputDeviceInfo(d.Name, d.ID));
        }

        public NativeSlice<float> GetChannelDataSlice()
        {
            try
            {
                return inputStream?.GetChannelDataSlice(0) ?? default;
            }
            catch (InvalidOperationException ex)
            {
                Debug.LogException(ex);
                inputStream = null;
                return default;
            }
        }

        public void Switch(AudioInputDeviceInfo audioInputDeviceInfo)
        {
            Assert.IsTrue(audioInputDeviceInfo.IsValid);
            var deviceDescriptor = AudioSystem.GetInputDevice(audioInputDeviceInfo.Id);
            if (!deviceDescriptor.IsValid)
            {
                Debug.LogError($"Invalid device: {audioInputDeviceInfo.Name}");
                return;
            }

            inputStream = AudioSystem.GetInputStream(deviceDescriptor);
            currentInputDevice.Value = audioInputDeviceInfo;
            PlayerPrefs.SetString(PrefsKey, audioInputDeviceInfo.Id);
            RectorLogger.AudioInputDevice(audioInputDeviceInfo.Id, audioInputDeviceInfo.Name);
        }

        // dbFS
        public Vector4 GetInputLevels()
        {
            if (inputStream == null) return Vector4.one * -60f;
            try
            {
                return new Vector4(
                    inputStream.GetChannelLevel(0, FilterType.Bypass),
                    inputStream.GetChannelLevel(0, FilterType.LowPass),
                    inputStream.GetChannelLevel(0, FilterType.BandPass),
                    inputStream.GetChannelLevel(0, FilterType.HighPass)
                );
            }
            catch (InvalidOperationException ex)
            {
                Debug.LogException(ex);
                inputStream = null;
                return Vector4.one * -60f;
            }
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
                    Switch(new AudioInputDeviceInfo(descriptor.Name, descriptor.ID));
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
