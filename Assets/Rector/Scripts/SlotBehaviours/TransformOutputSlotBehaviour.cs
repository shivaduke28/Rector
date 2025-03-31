using R3;
using Rector.NodeBehaviours;
using UnityEngine;

namespace Rector.SlotBehaviours
{
    // NOTE: Transform系まとめて一つにしてしまってもいいかも
    [AddComponentMenu("Rector Transform Output Slot")]
    public sealed class TransformOutputSlotBehaviour : OutputSlotBehaviour
    {
        IOutput[] outputs;

        public override IOutput[] GetOutputs()
        {
            return outputs ??= new IOutput[]
            {
                new ObservableOutput<Vector3>("Position", Observable.EveryValueChanged(transform, t => t.position, destroyCancellationToken)),
                new ObservableOutput<Transform>("Transform", Observable.Return(transform)),
            };
        }
    }
}
