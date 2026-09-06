using R3;

namespace Rector.UI.Graphs.Slots
{
    /// <summary>
    /// 値を保持しつつ、同じ値でも届くたびに流す int 入力スロット。
    /// Loop.Beat のように「同じ拍でも毎回評価したい」位置入力に使う。
    /// 接続時は現在値がリプレイされる（ReactivePropertyInputSlot と同じ）。
    /// </summary>
    public sealed class BehaviorSubjectIntInputSlot : InputSlot<int>, IIntValueInputSlot
    {
        readonly BehaviorSubject<int> subject;
        readonly int defaultValue;
        readonly ReadOnlyReactiveProperty<bool> isMuted;

        public int MinValue { get; }
        public int MaxValue { get; }

        public int Value
        {
            get => subject.Value;
            set => subject.OnNext(value);
        }

        public BehaviorSubjectIntInputSlot(NodeId nodeId, int index, string name, BehaviorSubject<int> subject,
            int defaultValue, int minValue, int maxValue, ReadOnlyReactiveProperty<bool> isMuted) : base(nodeId, index, name)
        {
            this.subject = subject;
            this.defaultValue = defaultValue;
            this.isMuted = isMuted;
            MinValue = minValue;
            MaxValue = maxValue;
        }

        public override void Send(int value)
        {
            if (isMuted.CurrentValue) return;
            subject.OnNext(value);
        }

        public override Observable<int> Observable() => subject;

        public override void Disconnected()
        {
            base.Disconnected();
            if (ConnectedCount == 0)
            {
                subject.OnNext(defaultValue);
            }
        }
    }
}
