using System.Linq;
using Rector.SlotBehaviours;
using Rector.UI.Graphs;
using UnityEngine;

namespace Rector.NodeBehaviours
{
    [AddComponentMenu("Rector/Node Behaviour")]
    public class NodeBehaviour : MonoBehaviour
    {
        [SerializeField] protected SlotBehaviour[] slotBehaviours;

        IInput[] inputs;
        IOutput[] outputs;

        public virtual string Category => NodeCategory.Scene;

        public virtual IInput[] GetInputs() => inputs ??= slotBehaviours.SelectMany(c => c.GetInputs()).ToArray();
        public virtual IOutput[] GetOutputs() => outputs ??= slotBehaviours.SelectMany(c => c.GetOutputs()).ToArray();

        public string Name => name;

        void Reset()
        {
            RetrieveComponents();
        }

        public virtual void RetrieveComponents()
        {
            slotBehaviours = GetComponentsInChildren<SlotBehaviour>();
        }
    }
}
