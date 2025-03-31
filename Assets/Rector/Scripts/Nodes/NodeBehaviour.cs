using System.Linq;
using UnityEngine;

namespace Rector.Nodes
{
    public class NodeBehaviour : MonoBehaviour
    {
        [SerializeField] InputBehaviour[] inputBehaviours;
        [SerializeField] OutputBehaviour[] outputBehaviours;

        public virtual IInput[] GetInputs() => inputBehaviours.SelectMany(input => input.GetInputs()).ToArray();
        public virtual IOutput[] GetOutputs() => outputBehaviours.SelectMany(output => output.GetOutputs()).ToArray();

        public string Name => name;

        void Reset()
        {
            inputBehaviours = GetComponentsInChildren<InputBehaviour>();
            outputBehaviours = GetComponentsInChildren<OutputBehaviour>();
        }
    }
}
