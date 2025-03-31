using R3;
using R3.Triggers;
using Rector.NodeBehaviours;

namespace Rector.NodeComponents
{
    public sealed class CollisionOutputBehaviour : OutputBehaviour
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
