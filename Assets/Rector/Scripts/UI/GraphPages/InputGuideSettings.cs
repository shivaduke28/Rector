using R3;
using UnityEngine;

namespace Rector.UI.GraphPages
{
    /// <summary>
    /// 操作ガイドの表示設定。値は PlayerPrefs に覚える。
    /// </summary>
    public sealed class InputGuideSettings
    {
        const string PrefsKey = "Rector_InputGuideMode";

        readonly ReactiveProperty<InputGuideMode> mode;

        /// <summary>ガイドの表記。既定はDualShock。Offなら出さない。</summary>
        public ReadOnlyReactiveProperty<InputGuideMode> Mode => mode;

        public InputGuideSettings()
        {
            // 保存された値が範囲外でも壊れないようclampして読む
            var saved = PlayerPrefs.GetInt(PrefsKey, (int)InputGuideMode.DualShock);
            mode = new ReactiveProperty<InputGuideMode>(
                (InputGuideMode)Mathf.Clamp(saved, (int)InputGuideMode.Off, (int)InputGuideMode.Keyboard));
        }

        public void SetMode(InputGuideMode value)
        {
            if (value == mode.Value) return;

            mode.Value = value;
            PlayerPrefs.SetInt(PrefsKey, (int)value);
        }
    }
}
