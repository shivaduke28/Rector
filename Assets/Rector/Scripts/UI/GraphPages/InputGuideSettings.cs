using R3;
using UnityEngine;

namespace Rector.UI.GraphPages
{
    /// <summary>
    /// 操作ガイドの表示設定。値は PlayerPrefs に覚える。
    /// </summary>
    public sealed class InputGuideSettings
    {
        const string PrefsKey = "Rector_InputGuideVisible";

        readonly ReactiveProperty<bool> visible;

        /// <summary>操作ガイドを表示するか。既定はオン。</summary>
        public ReadOnlyReactiveProperty<bool> Visible => visible;

        public InputGuideSettings()
        {
            // PlayerPrefs に bool はないので 0/1 で持つ
            visible = new ReactiveProperty<bool>(PlayerPrefs.GetInt(PrefsKey, 1) != 0);
        }

        public void SetVisible(bool value)
        {
            if (value == visible.Value) return;

            visible.Value = value;
            PlayerPrefs.SetInt(PrefsKey, value ? 1 : 0);
        }
    }
}
