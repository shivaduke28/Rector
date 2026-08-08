using R3;
using Rector.Midi;
using Rector.NodeBehaviours;
using Rector.UI.Graphs.Slots;

namespace Rector.UI.Graphs.Nodes
{
    public sealed class MidiNoteNode : MidiSourceNode
    {
        public const string NodeName = "MIDI Note";
        public override InputSlot[] InputSlots { get; }
        public override OutputSlot[] OutputSlots { get; }

        public MidiNoteNode(NodeId id, MidiModel midiModel)
            : base(id, NodeName, new IntInput("Note", 60, 0, 127), midiModel.NoteOn.Select(e => e.NoteNumber))
        {
            var matchedOn = midiModel.NoteOn.Where(e => e.NoteNumber == NumberInput.Value.Value);
            var matchedOff = midiModel.NoteOff.Where(e => e.NoteNumber == NumberInput.Value.Value);
            var noteOn = matchedOn.Where(_ => IsActive);
            var noteOff = matchedOff.Where(_ => IsActive);
            DisplayValue = matchedOn.Select(e => e.Velocity).Merge(matchedOff.Select(_ => 0f));

            InputSlots = new[]
            {
                SlotConverter.Convert(id, 0, ActiveInput, IsMuted),
                SlotConverter.Convert(id, 1, NumberInput, IsMuted),
                new CallbackInputSlot(id, 2, "Learn", ToggleLearn, IsMuted)
            };

            OutputSlots = new OutputSlot[]
            {
                new ObservableOutputSlot<Unit>(id, 0, "Note On", noteOn.AsUnitObservable(), IsMuted),
                new ObservableOutputSlot<Unit>(id, 1, "Note Off", noteOff.AsUnitObservable(), IsMuted),
                new ObservableOutputSlot<float>(id, 2, "Velocity", noteOn.Select(e => e.Velocity), IsMuted),
                new ObservableOutputSlot<bool>(id, 3, "Gate", noteOn.Select(_ => true).Merge(noteOff.Select(_ => false)).DistinctUntilChanged(), IsMuted)
            };
        }
    }
}
