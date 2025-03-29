using R3;
using Rector.UI.Graphs.Nodes;

namespace Rector.UI.NodeEdit
{
    public sealed class ExposedBoolInputModel : IExposedInputModel
    {
        public readonly ReactivePropertyInputSlot<bool> Slot;
        public readonly ReactiveProperty<bool> IsFocused = new(false);
        public readonly RectorToggleState ToggleState;

        public ExposedBoolInputModel(ReactivePropertyInputSlot<bool> slot)
        {
            Slot = slot;
            ToggleState = new RectorToggleState(slot.Property);
        }

        public void Set(bool value) => Slot.Property.Value = value;

        public void Toggle()
        {
            var value = Slot.Property.Value;
            Slot.Property.Value = !value;
        }

        public void Focus() => IsFocused.Value = true;

        public void Unfocus() => IsFocused.Value = false;
    }
}
