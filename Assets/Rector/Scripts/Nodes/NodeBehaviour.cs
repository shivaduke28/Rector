using System.Linq;
using UnityEngine;

namespace Rector.Nodes
{
    public sealed class NodeBehaviour : MonoBehaviour
    {
        [SerializeField] InputBehaviour[] inputBehaviours;
        [SerializeField] OutputBehaviour[] outputBehaviours;

        public IInput[] GetInputs() => inputBehaviours.SelectMany(input => input.GetInputs()).ToArray();
        public IOutput[] GetOutputs() => outputBehaviours.SelectMany(output => output.GetOutputs()).ToArray();

        public string Name => name;

        void Reset()
        {
            inputBehaviours = GetComponentsInChildren<InputBehaviour>();
            outputBehaviours = GetComponentsInChildren<OutputBehaviour>();
        }
    }
}
