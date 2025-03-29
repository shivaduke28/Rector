using R3;
using Rector.UI.Graphs.Nodes;
using Rector.UI.Graphs.Slots;

namespace Rector.UI.GraphPages.NodeParameters
{
    public sealed class ExposedCallbackInputModel : IExposedInputModel
    {
        public readonly CallbackInputSlot Slot;
        public readonly ReactiveProperty<bool> IsFocused = new(false);

        public ExposedCallbackInputModel(CallbackInputSlot slot) => Slot = slot;

        public void Invoke() => Slot.SendForce();

        public void Focus() => IsFocused.Value = true;

        public void Unfocus() => IsFocused.Value = false;
    }
}
