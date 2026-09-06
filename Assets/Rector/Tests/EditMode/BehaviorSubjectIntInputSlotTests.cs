using System.Collections.Generic;
using NUnit.Framework;
using R3;
using Rector.UI.Graphs;
using Rector.UI.Graphs.Slots;

namespace Rector.Tests.EditMode
{
    public sealed class BehaviorSubjectIntInputSlotTests
    {
        static BehaviorSubjectIntInputSlot Create(BehaviorSubject<int> subject, ReactiveProperty<bool> muted, int defaultValue = 0) =>
            new(NodeId.Generate(), 0, "Beat", subject, defaultValue, 0, 256, muted);

        [Test]
        public void ReplaysCurrentValueAndEmitsSameValueEveryTime()
        {
            var subject = new BehaviorSubject<int>(3);
            var slot = Create(subject, new ReactiveProperty<bool>(false));
            var received = new List<int>();
            using var subscription = slot.Observable().Subscribe(received.Add);

            Assert.That(received, Is.EqualTo(new[] { 3 }));

            slot.Send(3);
            slot.Send(3);
            Assert.That(received, Is.EqualTo(new[] { 3, 3, 3 }));
            Assert.That(slot.Value, Is.EqualTo(3));
        }

        [Test]
        public void ValueSetterFlowsThroughWireEvenWhenMuted()
        {
            var subject = new BehaviorSubject<int>(0);
            var muted = new ReactiveProperty<bool>(true);
            var slot = Create(subject, muted);
            var received = new List<int>();
            using var subscription = slot.Observable().Subscribe(received.Add);

            // Send はミュートで止まるが、HUD/復元が使う Value の代入は通る（ReactivePropertyInputSlot と同じ）
            slot.Send(5);
            Assert.That(received, Is.EqualTo(new[] { 0 }));

            slot.Value = 7;
            Assert.That(received, Is.EqualTo(new[] { 0, 7 }));
        }

        [Test]
        public void RestoresDefaultWhenLastEdgeIsDisconnected()
        {
            var subject = new BehaviorSubject<int>(0);
            var slot = Create(subject, new ReactiveProperty<bool>(false), defaultValue: 0);
            var received = new List<int>();
            using var subscription = slot.Observable().Subscribe(received.Add);

            slot.OnConnected();
            slot.OnConnected();
            slot.Send(4);
            slot.Disconnected();
            Assert.That(received, Is.EqualTo(new[] { 0, 4 }));

            slot.Disconnected();
            Assert.That(received, Is.EqualTo(new[] { 0, 4, 0 }));
        }
    }
}
