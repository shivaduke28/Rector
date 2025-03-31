using R3;
using Rector.NodeBehaviours;
using UnityEngine;

namespace Rector.NodeComponents
{
    public sealed class TransformOutputBehaviour : OutputBehaviour
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
