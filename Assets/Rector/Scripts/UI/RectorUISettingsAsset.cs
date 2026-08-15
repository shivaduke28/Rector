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
        public VisualTreeAsset exposedVector3Header;
        public VisualTreeAsset exposedVector3ComponentSlot;
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
        public Texture2D input;
        public Texture2D sequence;
        public Texture2D cameraFilled;
        public Texture2D vfxFilled;

        public Texture2D GetIcon(Graphs.NodeCategory category)
        {
            return category switch
            {
                Graphs.NodeCategory.Vfx => vfx,
                Graphs.NodeCategory.Camera => camera,
                Graphs.NodeCategory.Event => @event,
                Graphs.NodeCategory.Operator => @operator,
                Graphs.NodeCategory.Math => math,
                Graphs.NodeCategory.Scene => scene,
                Graphs.NodeCategory.System => system,
                Graphs.NodeCategory.Input => input,
                Graphs.NodeCategory.Sequence => sequence,
                _ => null
            };
        }

        // アクティブ状態表示用。未割当(null)のカテゴリは outline のまま
        public Texture2D GetFilledIcon(Graphs.NodeCategory category)
        {
            return category switch
            {
                Graphs.NodeCategory.Camera => cameraFilled,
                Graphs.NodeCategory.Vfx => vfxFilled,
                _ => null
            };
        }
    }
}
