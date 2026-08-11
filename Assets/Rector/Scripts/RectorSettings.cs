using System;
using Rector.NodeBehaviours;
using Rector.Vfx;
using UnityEngine;

namespace Rector
{
    [CreateAssetMenu(fileName = "RectorSettings", menuName = "Rector/SettingsAsset", order = 100)]
    public sealed class RectorSettingsAsset : ScriptableObject
    {
        public HudSettings hudSettings;
        public SceneSettings sceneSettings;
        public VfxSettings vfxSettings;
    }

    [Serializable]
    public sealed class HudSettings
    {
        [Tooltip("HUDヘッダに表示する名前。空なら Product Name を使う")]
        public string appName = "";

        [Tooltip("HUDヘッダに表示するバージョン。空なら Player Settings の Version を使う")]
        public string version = "1.0.0";
    }

    [Serializable]
    public sealed class SceneSettings
    {
        public string[] sceneNames = { };
    }

    [Serializable]
    public sealed class VfxSettings
    {
        public VfxNodeBehaviour[] vfxNodeBehaviours = { };
    }
}
