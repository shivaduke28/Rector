using UnityEngine;
using UnityEngine.UIElements;

namespace Rector.UI.Hud
{
    public sealed class HudContainer : MonoBehaviour
    {
        [SerializeField] UIDocument uiDocument;

        public VisualElement Root => uiDocument.rootVisualElement;
    }
}
