using R3;
using Rector.UI.Graphs.Nodes;
using UnityEngine;

namespace Rector.UI.NodeEdit
{
    public sealed class ExposedIntInputModel : IExposedInputModel
    {
        public readonly ReactivePropertyIntInputSlot Slot;
        public readonly ReactiveProperty<bool> IsFocused = new(false);

        readonly int delta;

        public ExposedIntInputModel(ReactivePropertyIntInputSlot slot)
        {
            Slot = slot;
            delta = 1;
        }

        public void Increment()
        {
            Slot.Property.Value += Mathf.Clamp(delta, 0, Slot.MaxValue - Slot.Property.Value);
        }

        public void Decrement()
        {
            Slot.Property.Value -= Mathf.Clamp(delta, 0, Slot.Property.Value - Slot.MinValue);
        }

        public void Focus() => IsFocused.Value = true;
        public void Unfocus() => IsFocused.Value = false;
    }
}