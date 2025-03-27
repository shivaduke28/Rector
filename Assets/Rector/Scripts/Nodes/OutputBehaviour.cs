using UnityEngine;

namespace Rector.Nodes
{
    public abstract class OutputBehaviour : MonoBehaviour
    {
        public abstract IOutput[] GetOutputs();
    }
}