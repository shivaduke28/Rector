using UnityEngine;

namespace Rector.Nodes
{
    public abstract class InputBehaviour : MonoBehaviour
    {
        public abstract IInput[] GetInputs();
    }
}
