using System;
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
        [SerializeField] string guid;
        [SerializeField] NodeCategory category = NodeCategory.Scene;

        IInput[] inputs;
        IOutput[] outputs;
        Guid? cachedGuid;

        public virtual NodeCategory Category => category;
        public Guid Guid => cachedGuid ??= string.IsNullOrEmpty(guid) ? Guid.Empty : Guid.Parse(guid);

        public virtual IInput[] GetInputs() => inputs ??= slotBehaviours.SelectMany(c => c.GetInputs()).ToArray();
        public virtual IOutput[] GetOutputs() => outputs ??= slotBehaviours.SelectMany(c => c.GetOutputs()).ToArray();

        public string Name => name;

        void Reset()
        {
            RetrieveComponents();
            if (string.IsNullOrEmpty(guid))
            {
                guid = Guid.NewGuid().ToString();
            }
        }

        public virtual void RetrieveComponents()
        {
            slotBehaviours = GetComponentsInChildren<SlotBehaviour>();
        }

        public void RegenerateGuid()
        {
            guid = Guid.NewGuid().ToString();
            cachedGuid = null;
        }
    }
}
