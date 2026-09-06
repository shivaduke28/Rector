using R3;
using Rector.UI.Graphs.Slots;
using UnityEngine;

namespace Rector.UI.GraphPages.NodeParameters
{
    public sealed class ExposedIntInputModel : IExposedInputModel
    {
        public readonly IIntValueInputSlot Slot;
        public readonly ReactiveProperty<bool> IsFocused = new(false);

        readonly int delta;

        public ExposedIntInputModel(IIntValueInputSlot slot)
        {
            Slot = slot;
            delta = 1;
        }

        public string Label => Slot.Name;

        public void Increment()
        {
            Slot.Value += Mathf.Clamp(delta, 0, Slot.MaxValue - Slot.Value);
        }

        public void Decrement()
        {
            Slot.Value -= Mathf.Clamp(delta, 0, Slot.Value - Slot.MinValue);
        }

        public void DoAction() { }

        public void Focus() => IsFocused.Value = true;
        public void Unfocus() => IsFocused.Value = false;
    }
}
