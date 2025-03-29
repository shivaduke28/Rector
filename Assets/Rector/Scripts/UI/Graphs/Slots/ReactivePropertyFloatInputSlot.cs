using R3;

namespace Rector.UI.Graphs.Slots
{
    public sealed class ReactivePropertyFloatInputSlot : ReactivePropertyInputSlot<float>
    {
        public readonly float MinValue;
        public readonly float MaxValue;

        public ReactivePropertyFloatInputSlot(NodeId nodeId, int index, string name, ReactiveProperty<float> property,
            float defaultValue, float minValue, float maxValue, ReadOnlyReactiveProperty<bool> isMuted) : base(nodeId, index, name, property, defaultValue, isMuted)
        {
            MinValue = minValue;
            MaxValue = maxValue;
        }
    }
}
