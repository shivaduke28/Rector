using System;
using UnityEngine.UIElements;

namespace Rector.UI
{
    public sealed class VisualElementFactory
    {
        static VisualElementFactory instance;
        readonly RectorUISettingsAsset settings;

        public static VisualElementFactory Instance
        {
            get
            {
                if (instance == null)
                {
                    throw new InvalidOperationException("Not initialized.");
                }
                return instance;
            }
        }

        VisualElementFactory(RectorUISettingsAsset settings)
        {
            this.settings = settings;
        }

        // call from installer at first
        public static void Initialize(RectorUISettingsAsset settings)
        {
            instance = new VisualElementFactory(settings);
        }

        public VisualElement CreateNode() => settings.node.Instantiate();
        public VisualElement CreateInputSlot() => settings.inputSlot.Instantiate();
        public VisualElement CreateOutputSlot() => settings.outputSlot.Instantiate();
        public VisualElement CreateExposedFloatSlot() => settings.exposedFloatSlot.Instantiate();
        public VisualElement CreateExposedIntSlot() => settings.exposedIntSlot.Instantiate();
        public VisualElement CreateExposedBoolSlot() => settings.exposedBoolSlot.Instantiate();
        public VisualElement CreateExposedCallbackSlot() => settings.exposedCallbackSlot.Instantiate();
        public VisualElement CreateConsoleLog() => settings.consoleLog.Instantiate();
    }
}
