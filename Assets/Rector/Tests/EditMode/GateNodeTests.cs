using System.Collections.Generic;
using NUnit.Framework;
using R3;
using Rector.UI.Graphs;
using Rector.UI.Graphs.Nodes;
using Rector.UI.Graphs.Slots;

namespace Rector.Tests.EditMode
{
    public sealed class GateNodeTests
    {
        [Test]
        public void OpenGatePassesEverything()
        {
            var node = new GateNode(NodeId.Generate());
            var input = (CallbackFloatInputSlot)node.InputSlots[0];
            var output = (ObservableOutputSlot<float>)node.OutputSlots[0];
            var received = new List<float>();
            using var subscription = output.Observable().Subscribe(received.Add);

            input.Send(1f);
            input.Send(0f);
            input.Send(2.5f);

            Assert.That(received, Is.EqualTo(new[] { 1f, 0f, 2.5f }));
        }

        [Test]
        public void ClosedGateBlocksEventsButLetsZeroThrough()
        {
            var node = new GateNode(NodeId.Generate());
            var input = (CallbackFloatInputSlot)node.InputSlots[0];
            var gate = (ReactivePropertyInputSlot<bool>)node.InputSlots[1];
            var output = (ObservableOutputSlot<float>)node.OutputSlots[0];
            var received = new List<float>();
            using var subscription = output.Observable().Subscribe(received.Add);

            gate.Property.Value = false;
            input.Send(1f);
            // 消灯信号(0)は閉じていても通る。止めると下流の VFX が点いたまま残る
            input.Send(0f);
            input.Send(3f);

            Assert.That(received, Is.EqualTo(new[] { 0f }));
        }
    }
}
