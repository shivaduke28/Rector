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
    }
}
