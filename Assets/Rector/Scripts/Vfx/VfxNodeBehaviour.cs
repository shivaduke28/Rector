using System.Linq;
using Rector.NodeBehaviours;
using UnityEngine;

namespace Rector.Vfx
{
    [AddComponentMenu("Rector/VFX Node Behaviour")]
    [RequireComponent(typeof(VfxInputSlotBehaviour))]
    public class VfxNodeBehaviour : NodeBehaviour
    {
        [SerializeField] VfxInputSlotBehaviour vfxInputSlot;

        public void ToggleActive()
        {
            vfxInputSlot.ToggleActive();
        }

        public override void RetrieveComponents()
        {
            base.RetrieveComponents();
            vfxInputSlot = GetComponent<VfxInputSlotBehaviour>();
            if (vfxInputSlot != null)
            {
                var list = slotBehaviours.ToList();
                var i = list.IndexOf(vfxInputSlot);
                if (i > 0)
                {
                    list.RemoveAt(i);
                    list.Insert(0, vfxInputSlot);
                }

                slotBehaviours = list.ToArray();
            }
        }
    }
}
