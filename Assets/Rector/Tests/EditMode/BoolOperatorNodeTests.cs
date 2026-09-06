using System.Collections.Generic;
using NUnit.Framework;
using R3;
using Rector.UI.Graphs;
using Rector.UI.Graphs.Nodes;
using Rector.UI.Graphs.Slots;

namespace Rector.Tests.EditMode
{
    public sealed class BoolOperatorNodeTests
    {
        static IValueInputSlot<bool> BoolInput(Node node, int index) => (IValueInputSlot<bool>)node.InputSlots[index];
        static ObservableOutputSlot<bool> BoolOutput(Node node) => (ObservableOutputSlot<bool>)node.OutputSlots[0];

        [Test]
        public void NegateEmitsOnEveryArrivalEvenWhenInputRepeats()
        {
            var node = new NegateNode(NodeId.Generate());
            var received = new List<bool>();
            using var subscription = BoolOutput(node).Observable().Subscribe(received.Add);

            // 接続時に現在値（!false）がリプレイされる
            Assert.That(received, Is.EqualTo(new[] { true }));

            var input = BoolInput(node, 0);
            input.Send(false);
            input.Send(false);
            input.Send(true);
            input.Send(true);

            Assert.That(received, Is.EqualTo(new[] { true, true, true, false, false }));
        }

        [Test]
        public void AndReEvaluatesWheneverEitherInputArrives()
        {
            var node = new AndNode(NodeId.Generate());
            var received = new List<bool>();
            using var subscription = BoolOutput(node).Observable().Subscribe(received.Add);
            Assert.That(received, Is.EqualTo(new[] { false }));

            BoolInput(node, 0).Send(true);
            BoolInput(node, 1).Send(true);
            BoolInput(node, 1).Send(true);
            BoolInput(node, 0).Send(false);
            BoolInput(node, 0).Send(false);

            Assert.That(received, Is.EqualTo(new[] { false, false, true, true, false, false }));
        }

        [Test]
        public void OrReEvaluatesWheneverEitherInputArrives()
        {
            var node = new OrNode(NodeId.Generate());
            var received = new List<bool>();
            using var subscription = BoolOutput(node).Observable().Subscribe(received.Add);
            Assert.That(received, Is.EqualTo(new[] { true }));

            BoolInput(node, 0).Send(false);
            BoolInput(node, 1).Send(false);
            BoolInput(node, 1).Send(false);
            BoolInput(node, 0).Send(true);

            Assert.That(received, Is.EqualTo(new[] { true, true, false, false, true }));
        }

        [Test]
        public void ValueSetterWritesThroughAndDisconnectRestoresDefault()
        {
            var node = new OrNode(NodeId.Generate());
            var input = BoolInput(node, 0);
            var received = new List<bool>();
            using var subscription = input.Observable().Subscribe(received.Add);
            Assert.That(received, Is.EqualTo(new[] { true }));

            input.Value = false;
            Assert.That(input.Value, Is.False);

            input.OnConnected();
            input.Disconnected();
            Assert.That(received, Is.EqualTo(new[] { true, false, true }));
        }
    }
}
