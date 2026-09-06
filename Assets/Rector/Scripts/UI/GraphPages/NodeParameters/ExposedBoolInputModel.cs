using R3;
using Rector.UI.Graphs.Slots;

namespace Rector.UI.GraphPages.NodeParameters
{
    public sealed class ExposedBoolInputModel : IExposedInputModel
    {
        public readonly IValueInputSlot<bool> Slot;
        public readonly ReactiveProperty<bool> IsFocused = new(false);

        // トグルは ReactiveProperty を要求するので、スロットの裏が何であれ表示用の値を挟む。
        // スロットとの同期は View が Bind の寿命で張る（ExposedIntInputView と同じ方針）
        public readonly ReactiveProperty<bool> DisplayValue;
        public readonly RectorToggleState ToggleState;

        public ExposedBoolInputModel(IValueInputSlot<bool> slot)
        {
            Slot = slot;
            DisplayValue = new ReactiveProperty<bool>(slot.Value);
            ToggleState = new RectorToggleState(DisplayValue);
        }

        public string Label => Slot.Name;

        public void Set(bool value) => Slot.Value = value;

        public void Toggle() => Set(!Slot.Value);

        public void Increment() => Set(true);

        public void Decrement() => Set(false);

        public void DoAction() => Toggle();

        public void Focus() => IsFocused.Value = true;

        public void Unfocus() => IsFocused.Value = false;
    }
}
