using System.Linq;
using Rector.NodeComponents;
using UnityEngine;

namespace Rector.Nodes
{
    [RequireComponent(typeof(VfxInputBehaviour))]
    public class VfxNodeBehaviour : MonoBehaviour
    {
        [SerializeField] VfxInputBehaviour vfxInputBehaviour;
        [SerializeField] InputBehaviour[] inputBehaviours;
        [SerializeField] OutputBehaviour[] outputBehaviours;

        public IInput[] GetInputs() => inputBehaviours.Prepend(vfxInputBehaviour).SelectMany(input => input.GetInputs()).ToArray();
        public IOutput[] GetOutputs() => outputBehaviours.SelectMany(output => output.GetOutputs()).ToArray();

        public string Name => name;
        public void SetActive(bool value) => vfxInputBehaviour.SetActive(value);

        void Reset()
        {
            vfxInputBehaviour = GetComponent<VfxInputBehaviour>();
            inputBehaviours = GetComponentsInChildren<InputBehaviour>().Where(x => x != vfxInputBehaviour).ToArray();
            outputBehaviours = GetComponentsInChildren<OutputBehaviour>();
        }
    }
}
