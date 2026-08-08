using System;
using NUnit.Framework;
using R3;
using Rector.UI.GraphPages;
using Rector.UI.Graphs;
using Rector.UI.Graphs.Slots;

namespace Rector.Tests.EditMode
{
    /// <summary>
    /// SlotNavigatorのカーソル移動先決定のテスト。ビュー非依存なので、スロットだけ手で並べて検証する。
    /// </summary>
    public sealed class SlotNavigatorTests
    {
        sealed class TestInputSlot : InputSlot<Unit>
        {
            public TestInputSlot(int index) : base(new NodeId(0), index, $"in{index}")
            {
            }

            public override void Send(Unit value)
            {
            }

            public override Observable<Unit> Observable() => R3.Observable.Empty<Unit>();
        }

        sealed class TestOutputSlot : OutputSlot<Unit>
        {
            public TestOutputSlot(int index) : base(new NodeId(0), index, $"out{index}")
            {
            }

            public override Observable<Unit> Observable() => R3.Observable.Empty<Unit>();
        }

        static InputSlot[] Inputs(int count)
        {
            var slots = new InputSlot[count];
            for (var i = 0; i < count; i++) slots[i] = new TestInputSlot(i);
            return slots;
        }

        static OutputSlot[] Outputs(int count)
        {
            var slots = new OutputSlot[count];
            for (var i = 0; i < count; i++) slots[i] = new TestOutputSlot(i);
            return slots;
        }

        [Test]
        public void PickInDirection_Output_ReturnsFirstOutput()
        {
            var inputs = Inputs(3);
            var outputs = Outputs(2);

            var slot = SlotNavigator.PickInDirection(SlotDirection.Output, inputs, outputs);

            Assert.That(slot, Is.SameAs(outputs[0]));
        }

        [Test]
        public void PickInDirection_Input_ReturnsFirstInput()
        {
            var inputs = Inputs(3);
            var outputs = Outputs(2);

            var slot = SlotNavigator.PickInDirection(SlotDirection.Input, inputs, outputs);

            Assert.That(slot, Is.SameAs(inputs[0]));
        }

        [Test]
        public void PickInDirection_OutputWithNoOutputs_FallsBackToInput()
        {
            var inputs = Inputs(2);

            var slot = SlotNavigator.PickInDirection(SlotDirection.Output, inputs, Array.Empty<OutputSlot>());

            Assert.That(slot, Is.SameAs(inputs[0]));
        }

        [Test]
        public void PickInDirection_InputWithNoInputs_FallsBackToOutput()
        {
            var outputs = Outputs(2);

            var slot = SlotNavigator.PickInDirection(SlotDirection.Input, Array.Empty<InputSlot>(), outputs);

            Assert.That(slot, Is.SameAs(outputs[0]));
        }

        [Test]
        public void PickInDirection_NoSlots_ReturnsNull()
        {
            Assert.That(SlotNavigator.PickInDirection(SlotDirection.Output, Array.Empty<InputSlot>(), Array.Empty<OutputSlot>()), Is.Null);
            Assert.That(SlotNavigator.PickInDirection(SlotDirection.Input, Array.Empty<InputSlot>(), Array.Empty<OutputSlot>()), Is.Null);
        }
    }
}
