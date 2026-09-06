using NUnit.Framework;
using R3;
using Rector.UI.Graphs;
using Rector.UI.Graphs.Nodes;
using Rector.UI.Graphs.Slots;

namespace Rector.Tests.EditMode
{
    public sealed class ChanceNodeTests
    {
        [Test]
        public void SlotsAreInAndChanceWithHalfAsDefault()
        {
            var node = new ChanceNode(NodeId.Generate());

            Assert.That(node.InputSlots[0].Name, Is.EqualTo("In"));

            var chance = node.InputSlots[1] as ReactivePropertyFloatInputSlot;
            Assert.That(chance, Is.Not.Null);
            Assert.That(chance.Name, Is.EqualTo("Chance"));
            Assert.That(chance.Property.Value, Is.EqualTo(0.5f));
            Assert.That(chance.MinValue, Is.EqualTo(0f));
            Assert.That(chance.MaxValue, Is.EqualTo(1f));
            Assert.That(node.Chance.CurrentValue, Is.EqualTo(0.5f));

            Assert.That(node.OutputSlots[0].Name, Is.EqualTo("Out"));
            Assert.That(node.OutputSlots[1].Name, Is.EqualTo("Hit"));

            var initialHit = true;
            using var subscription = node.Hit.Subscribe(x => initialHit = x);
            Assert.That(initialHit, Is.False);
        }

        [Test]
        public void HitReplaysCurrentValueAndEmitsEveryRoll()
        {
            var node = new ChanceNode(NodeId.Generate());
            var input = (CallbackFloatInputSlot)node.InputSlots[0];
            var chance = (ReactivePropertyFloatInputSlot)node.InputSlots[1];
            var hit = (ObservableOutputSlot<bool>)node.OutputSlots[1];

            var received = new System.Collections.Generic.List<bool>();
            using var subscription = hit.Observable().Subscribe(received.Add);

            // レベル出力なので接続時に初期値 false がリプレイされる
            Assert.That(received, Is.EqualTo(new[] { false }));

            // 当たりが続いても抽選ごとに毎回流れる
            chance.Property.Value = 1f;
            input.Send(1f);
            input.Send(2f);
            Assert.That(received, Is.EqualTo(new[] { false, true, true }));

            chance.Property.Value = 0f;
            input.Send(3f);
            Assert.That(received, Is.EqualTo(new[] { false, true, true, false }));
        }

        [Test]
        public void HitIsUpdatedBeforeOutFires()
        {
            var node = new ChanceNode(NodeId.Generate());
            var input = (CallbackFloatInputSlot)node.InputSlots[0];
            var chance = (ReactivePropertyFloatInputSlot)node.InputSlots[1];
            var output = (ObservableOutputSlot<float>)node.OutputSlots[0];

            chance.Property.Value = 1f;
            var latestHit = false;
            var hitSeenFromOut = false;
            using var hitSubscription = node.Hit.Subscribe(x => latestHit = x);
            using var subscription = output.Observable().Subscribe(_ => hitSeenFromOut = latestHit);

            input.Send(1f);

            Assert.That(hitSeenFromOut, Is.True);
        }

        [Test]
        public void ZeroChanceNeverPasses()
        {
            var node = new ChanceNode(NodeId.Generate());
            var input = (CallbackFloatInputSlot)node.InputSlots[0];
            var chance = (ReactivePropertyFloatInputSlot)node.InputSlots[1];
            var output = (ObservableOutputSlot<float>)node.OutputSlots[0];

            chance.Property.Value = 0f;
            var fired = 0;
            using var subscription = output.Observable().Subscribe(_ => fired++);

            for (var i = 0; i < 100; i++) input.Send(i);

            Assert.That(fired, Is.EqualTo(0));
        }

        [Test]
        public void FullChanceAlwaysPassesWithSameValue()
        {
            var node = new ChanceNode(NodeId.Generate());
            var input = (CallbackFloatInputSlot)node.InputSlots[0];
            var chance = (ReactivePropertyFloatInputSlot)node.InputSlots[1];
            var output = (ObservableOutputSlot<float>)node.OutputSlots[0];

            chance.Property.Value = 1f;
            var fired = 0;
            var last = float.NaN;
            using var subscription = output.Observable().Subscribe(x =>
            {
                fired++;
                last = x;
            });

            for (var i = 0; i < 100; i++) input.Send(i * 0.5f);

            Assert.That(fired, Is.EqualTo(100));
            Assert.That(last, Is.EqualTo(49.5f));
        }
    }
}
