using R3;
using Rector.Midi;
using Rector.NodeBehaviours;
using Rector.UI.Graphs.Slots;

namespace Rector.UI.Graphs.Nodes
{
    public sealed class MidiCcNode : MidiSourceNode
    {
        public const string NodeName = "MIDI CC";
        public override InputSlot[] InputSlots { get; }
        public override OutputSlot[] OutputSlots { get; }

        public MidiCcNode(NodeId id, MidiModel midiModel)
            : base(id, NodeName, new IntInput("CC", 1, 0, 127), midiModel.ControlChange.Select(e => e.ControlNumber))
        {
            var matched = midiModel.ControlChange.Where(e => e.ControlNumber == NumberInput.Value.Value);
            DisplayValue = matched.Select(e => e.Value);

            InputSlots = new[]
            {
                SlotConverter.Convert(id, 0, ActiveInput, IsMuted),
                SlotConverter.Convert(id, 1, NumberInput, IsMuted),
                SlotConverter.Convert(id, 2, LearnInput, IsMuted)
            };

            OutputSlots = new OutputSlot[]
            {
                new ObservableOutputSlot<float>(id, 0, "Value",
                    matched.Where(_ => IsActive).Select(e => e.Value), IsMuted)
            };
        }
    }
}
