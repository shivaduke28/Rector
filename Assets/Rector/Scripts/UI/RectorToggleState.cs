using R3;

namespace Rector.UI
{
    public sealed class RectorToggleState
    {
        public readonly ReactiveProperty<bool> Value;

        public RectorToggleState(bool value)
        {
            Value = new ReactiveProperty<bool>(value);
        }

        public RectorToggleState(ReactiveProperty<bool> value)
        {
            Value = value;
        }
    }
}
