using System;
using Rector.NodeBehaviours;
using UnityEngine;

namespace Rector
{
    [CreateAssetMenu(fileName = "RectorSettings", menuName = "Rector/SettingsAsset", order = 100)]
    public sealed class RectorSettingsAsset : ScriptableObject
    {
        public SceneSettings sceneSettings;
        public VfxSettings vfxSettings;
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
