using UnityEngine;

namespace Rector.NodeBehaviours
{
    public abstract class InputBehaviour : MonoBehaviour
    {
        public abstract IInput[] GetInputs();
    }
}
