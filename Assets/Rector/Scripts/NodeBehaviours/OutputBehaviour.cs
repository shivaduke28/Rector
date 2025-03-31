using UnityEngine;

namespace Rector.NodeBehaviours
{
    public abstract class OutputBehaviour : MonoBehaviour
    {
        public abstract IOutput[] GetOutputs();
    }
}