using R3;

namespace Rector.UI.Graphs.Slots
{
    public sealed class BehaviorSubjectIntInputSlot : BehaviorSubjectInputSlot<int>, IIntValueInputSlot
    {
        public int MinValue { get; }
        public int MaxValue { get; }

        public BehaviorSubjectIntInputSlot(NodeId nodeId, int index, string name, BehaviorSubject<int> subject,
            int defaultValue, int minValue, int maxValue, ReadOnlyReactiveProperty<bool> isMuted) : base(nodeId, index, name, subject, defaultValue, isMuted)
        {
            MinValue = minValue;
            MaxValue = maxValue;
        }
    }
}
