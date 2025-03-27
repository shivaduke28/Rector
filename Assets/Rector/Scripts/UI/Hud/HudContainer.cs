using Rector.UI.Graphs;
using UnityEngine;
using UnityEngine.UIElements;

namespace Rector.UI.Hud
{
    public sealed class HudContainer : MonoBehaviour
    {
        [SerializeField] UIDocument uiDocument;

        HudView view;

        public HudView GetHudView(UIInput uiInput, NodeTemplateRepository nodeTemplateRepository)
        {
            return view ??= new HudView(uiDocument.rootVisualElement, uiInput, nodeTemplateRepository);
        }
    }
}
