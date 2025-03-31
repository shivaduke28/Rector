using System.Linq;
using Rector.NodeBehaviours;
using UnityEngine;

namespace Rector.Vfx
{
    [RequireComponent(typeof(VfxInputSlot))]
    public class VfxNodeBehaviour : NodeBehaviour
    {
        [SerializeField] VfxInputSlot vfxInputSlot;

        public void ToggleActive()
        {
            vfxInputSlot.ToggleActive();
        }

        public override void RetrieveComponents()
        {
            base.RetrieveComponents();
            vfxInputSlot = GetComponent<VfxInputSlot>();
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
