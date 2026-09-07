using System;
using System.Collections.Generic;
using NUnit.Framework;
using R3;
using Rector.UI.Graphs;
using Rector.UI.Graphs.Nodes;
using Rector.UI.Graphs.Slots;

namespace Rector.Tests.EditMode
{
    public sealed class LoopNodeTests
    {
        static (LoopNode node, BehaviorSubjectIntInputSlot beat, ReactivePropertyInputSlot<int> length) Create()
        {
            var node = new LoopNode(NodeId.Generate());
            return (node, (BehaviorSubjectIntInputSlot)node.InputSlots[0], (ReactivePropertyInputSlot<int>)node.InputSlots[1]);
        }

        static List<T> Collect<T>(LoopNode node, int slot, ICollection<IDisposable> disposables)
        {
            var received = new List<T>();
            disposables.Add(((ObservableOutputSlot<T>)node.OutputSlots[slot]).Observable().Subscribe(received.Add));
            return received;
        }

        [Test]
        public void CycleFlowsOnlyAtHeadOfEachCycle()
        {
            var (node, beat, _) = Create();
            var disposables = new List<IDisposable>();
            var cycle = Collect<int>(node, 0, disposables);

            for (var b = 1; b <= 9; b++)
            {
                beat.Send(b);
            }

            Assert.That(cycle, Is.EqualTo(new[] { 1, 2, 3 }));
            disposables.ForEach(d => d.Dispose());
        }

        [Test]
        public void PhaseAndOnOffFlowEveryBeat()
        {
            var (node, beat, _) = Create();
            var disposables = new List<IDisposable>();
            var phase = Collect<int>(node, 1, disposables);
            var on = Collect<bool>(node, 2, disposables);
            var off = Collect<bool>(node, 3, disposables);

            for (var b = 1; b <= 5; b++)
            {
                beat.Send(b);
            }

            Assert.That(phase, Is.EqualTo(new[] { 1, 2, 3, 4, 1 }));
            Assert.That(on, Is.EqualTo(new[] { true, false, false, false, true }));
            Assert.That(off, Is.EqualTo(new[] { false, true, true, true, false }));
            disposables.ForEach(d => d.Dispose());
        }

        [Test]
        public void LengthOneFlowsCycleEveryBeatEvenWhenBeatRepeats()
        {
            var (node, beat, length) = Create();
            length.Property.Value = 1;
            var disposables = new List<IDisposable>();
            var cycle = Collect<int>(node, 0, disposables);

            beat.Send(1);
            beat.Send(1);
            beat.Send(2);

            Assert.That(cycle, Is.EqualTo(new[] { 1, 1, 2 }));
            disposables.ForEach(d => d.Dispose());
        }

        [Test]
        public void BeatZeroFlowsNothing()
        {
            var (node, beat, _) = Create();
            var disposables = new List<IDisposable>();
            var cycle = Collect<int>(node, 0, disposables);
            var phase = Collect<int>(node, 1, disposables);

            beat.Send(0);

            Assert.That(cycle, Is.Empty);
            Assert.That(phase, Is.Empty);
            disposables.ForEach(d => d.Dispose());
        }
    }
}
