using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace Rector.UI
{
    [CreateAssetMenu(fileName = "RectorUISettings", menuName = "Rector/UISettings")]
    public sealed class RectorUISettingsAsset : ScriptableObject
    {
        public VisualTreeAsset node;
        public VisualTreeAsset inputSlot;
        public VisualTreeAsset outputSlot;
        public VisualTreeAsset exposedFloatSlot;
        public VisualTreeAsset exposedIntSlot;
        public VisualTreeAsset exposedBoolSlot;
        public VisualTreeAsset exposedCallbackSlot;
        public VisualTreeAsset consoleLog;

        public RectorIconSettings iconSettings;
    }

    [Serializable]
    public sealed class RectorIconSettings
    {
        public Texture2D vfx;
        public Texture2D camera;
        public Texture2D math;
        public Texture2D @event;
        public Texture2D @operator;
        public Texture2D scene;
        public Texture2D system;
    }
}
