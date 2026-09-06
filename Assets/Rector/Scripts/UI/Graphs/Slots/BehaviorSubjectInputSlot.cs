using R3;

namespace Rector.UI.Graphs.Slots
{
    /// <summary>
    /// 値を保持しつつ、同じ値でも届くたびに流す入力スロット。
    /// Loop.Beat のような位置入力や、Negate/And/Or のように到着ごとに再評価したい bool 入力に使う。
    /// 接続時は現在値がリプレイされる（ReactivePropertyInputSlot と同じ）。
    /// </summary>
    public class BehaviorSubjectInputSlot<T> : InputSlot<T>, IValueInputSlot<T>
    {
        readonly BehaviorSubject<T> subject;
        readonly T defaultValue;
        readonly ReadOnlyReactiveProperty<bool> isMuted;

        public T Value
        {
            get => subject.Value;
            set => subject.OnNext(value);
        }

        public BehaviorSubjectInputSlot(NodeId nodeId, int index, string name, BehaviorSubject<T> subject,
            T defaultValue, ReadOnlyReactiveProperty<bool> isMuted) : base(nodeId, index, name)
        {
            this.subject = subject;
            this.defaultValue = defaultValue;
            this.isMuted = isMuted;
        }

        public override void Send(T value)
        {
            if (isMuted.CurrentValue) return;
            subject.OnNext(value);
        }

        public override Observable<T> Observable() => subject;

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
