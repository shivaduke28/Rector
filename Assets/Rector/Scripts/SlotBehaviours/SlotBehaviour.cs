using System;
using Rector.NodeBehaviours;
using UnityEngine;

namespace Rector.SlotBehaviours
{
    public abstract class SlotBehaviour : MonoBehaviour
    {
        public abstract IInput[] GetInputs();
        public abstract IOutput[] GetOutputs();
    }

    public abstract class InputSlotBehaviour : SlotBehaviour
    {
        public sealed override IOutput[] GetOutputs() => Array.Empty<IOutput>();
    }

    public abstract class OutputSlotBehaviour : SlotBehaviour
    {
        public sealed override IInput[] GetInputs() => Array.Empty<IInput>();
    }
}
