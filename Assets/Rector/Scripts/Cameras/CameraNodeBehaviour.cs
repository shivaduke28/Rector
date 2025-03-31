using System.Linq;
using R3;
using Rector.NodeBehaviours;
using UnityEngine;

namespace Rector.Cameras
{
    [AddComponentMenu("Rector/Camera Node Behaviour")]
    [RequireComponent(typeof(CameraInputSlotBehaviour))]
    public class CameraNodeBehaviour : NodeBehaviour
    {
        [SerializeField] CameraInputSlotBehaviour cameraInputSlot;

        public ReactiveProperty<bool> IsActive => cameraInputSlot.ActiveInput.Value;


        public override void RetrieveComponents()
        {
            cameraInputSlot = GetComponent<CameraInputSlotBehaviour>();
            if (cameraInputSlot == null)
            {
                cameraInputSlot = gameObject.AddComponent<CameraInputSlotBehaviour>();
            }

            base.RetrieveComponents();
            var list = slotBehaviours.ToList();
            var i = list.IndexOf(cameraInputSlot);
            if (i > 0)
            {
                list.RemoveAt(i);
                list.Insert(0, cameraInputSlot);
            }

            slotBehaviours = list.ToArray();
        }
    }
}
