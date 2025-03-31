using R3;
using R3.Triggers;
using Rector.NodeBehaviours;
using UnityEngine;

namespace Rector.SlotBehaviours
{
    [AddComponentMenu("Rector/Collision Output Slot")]
    public sealed class CollisionOutputSlotBehaviour : OutputSlotBehaviour
    {
        IOutput[] outputs;

        public override IOutput[] GetOutputs()
        {
            return outputs ??= new IOutput[]
            {
                new ObservableOutput<Unit>("Collision", gameObject.OnCollisionEnterAsObservable().AsUnitObservable())
            };
        }
    }
}
