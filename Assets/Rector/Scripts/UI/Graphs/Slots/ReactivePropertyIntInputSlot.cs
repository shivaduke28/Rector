using R3;

namespace Rector.UI.Graphs.Slots
{
    public sealed class ReactivePropertyIntInputSlot : ReactivePropertyInputSlot<int>, IIntValueInputSlot
    {
        public int MinValue { get; }
        public int MaxValue { get; }

        public int Value
        {
            get => Property.Value;
            set => Property.Value = value;
        }

        public ReactivePropertyIntInputSlot(NodeId nodeId, int index, string name, ReactiveProperty<int> property,
            int defaultValue, int minValue, int maxValue, ReadOnlyReactiveProperty<bool> isMuted) : base(nodeId, index, name, property, defaultValue, isMuted)
        {
            MinValue = minValue;
            MaxValue = maxValue;
        }
    }
}
