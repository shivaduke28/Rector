using NUnit.Framework;
using R3;
using Rector.Audio;

namespace Rector.Tests.EditMode
{
    public sealed class SequenceModelTests
    {
        [Test]
        public void BeatStartsAtOne()
        {
            using var model = new SequenceModel();
            Assert.That(model.BeatProperty.CurrentValue, Is.EqualTo(1));
        }

        [Test]
        public void StepWrapsAtLength()
        {
            using var model = new SequenceModel();
            model.SetLength(4);

            model.Step();
            model.Step();
            model.Step();
            Assert.That(model.BeatProperty.CurrentValue, Is.EqualTo(4));

            model.Step();
            Assert.That(model.BeatProperty.CurrentValue, Is.EqualTo(1));
        }

        [Test]
        public void LengthOneFiresEveryStep()
        {
            using var model = new SequenceModel();
            model.SetLength(1);

            var fired = 0;
            using var subscription = model.BeatProperty.Subscribe(_ => fired++);
            var initialReplay = fired;

            model.Step();
            model.Step();
            model.Step();

            Assert.That(model.BeatProperty.CurrentValue, Is.EqualTo(1));
            Assert.That(fired - initialReplay, Is.EqualTo(3));
        }

        [Test]
        public void ResetSetsBeatToOneAndFires()
        {
            using var model = new SequenceModel();
            model.SetLength(8);
            model.Step();
            model.Step();

            var fired = 0;
            using var subscription = model.BeatProperty.Subscribe(_ => fired++);
            var initialReplay = fired;

            model.Reset();

            Assert.That(model.BeatProperty.CurrentValue, Is.EqualTo(1));
            Assert.That(fired - initialReplay, Is.EqualTo(1));
        }

        [Test]
        public void SetLengthClampsToRange()
        {
            using var model = new SequenceModel();

            model.SetLength(0);
            Assert.That(model.LengthProperty.CurrentValue, Is.EqualTo(SequenceModel.MinLength));

            model.SetLength(10000);
            Assert.That(model.LengthProperty.CurrentValue, Is.EqualTo(SequenceModel.MaxLength));
        }

        [Test]
        public void SetLengthDoesNotTouchBeatAndStepRecovers()
        {
            using var model = new SequenceModel();
            model.SetLength(64);
            for (var i = 0; i < 10; i++)
            {
                model.Step();
            }

            Assert.That(model.BeatProperty.CurrentValue, Is.EqualTo(11));

            var fired = 0;
            using var subscription = model.BeatProperty.Subscribe(_ => fired++);
            var initialReplay = fired;

            model.SetLength(4);
            Assert.That(model.BeatProperty.CurrentValue, Is.EqualTo(11));
            Assert.That(fired - initialReplay, Is.EqualTo(0));

            model.Step();
            Assert.That(model.BeatProperty.CurrentValue, Is.EqualTo(4));
        }
    }
}
