using NUnit.Framework;
using R3;
using Rector.Audio;
using Rector.UI.Graphs;
using Rector.UI.Graphs.Nodes;
using Rector.UI.Graphs.Slots;

namespace Rector.Tests.EditMode
{
    public sealed class SequenceNodeTests
    {
        [Test]
        public void ActiveSlotIsFirstAndDefaultsToTrue()
        {
            using var model = new SequenceModel();
            using var node = new SequenceNode(NodeId.Generate(), model);

            var active = node.InputSlots[0] as ReactivePropertyInputSlot<bool>;
            Assert.That(active, Is.Not.Null);
            Assert.That(active.Name, Is.EqualTo("Active"));
            Assert.That(active.Property.Value, Is.True);
        }

        [Test]
        public void InactiveFreezesOutputWhileModelAdvances()
        {
            using var model = new SequenceModel();
            using var node = new SequenceNode(NodeId.Generate(), model);
            model.SetLength(8);

            var active = (ReactivePropertyInputSlot<bool>)node.InputSlots[0];
            var output = (ObservableOutputSlot<int>)node.OutputSlots[0];

            active.Property.Value = false;

            var fired = 0;
            // 非active中の購読はリプレイもゲートされる
            using var subscription = output.Observable().Subscribe(_ => fired++);
            Assert.That(fired, Is.EqualTo(0));

            model.Step();
            model.Step();
            Assert.That(fired, Is.EqualTo(0));
            Assert.That(model.BeatProperty.CurrentValue, Is.EqualTo(3));

            // 復帰時は再発火せず、次のStepから出力再開
            active.Property.Value = true;
            Assert.That(fired, Is.EqualTo(0));

            model.Step();
            Assert.That(fired, Is.EqualTo(1));
        }

        [Test]
        public void ActiveOutputFiresEveryStep()
        {
            using var model = new SequenceModel();
            using var node = new SequenceNode(NodeId.Generate(), model);
            model.SetLength(1);

            var output = (ObservableOutputSlot<int>)node.OutputSlots[0];

            var fired = 0;
            using var subscription = output.Observable().Subscribe(_ => fired++);
            var initialReplay = fired;

            model.Step();
            model.Step();
            Assert.That(fired - initialReplay, Is.EqualTo(2));
        }

        [Test]
        public void DoActionTogglesActiveState()
        {
            using var model = new SequenceModel();
            using var node = new SequenceNode(NodeId.Generate(), model);

            Assert.That(node.ActiveState.CurrentValue, Is.True);
            node.DoAction();
            Assert.That(node.ActiveState.CurrentValue, Is.False);
            node.DoAction();
            Assert.That(node.ActiveState.CurrentValue, Is.True);
        }
    }
}
