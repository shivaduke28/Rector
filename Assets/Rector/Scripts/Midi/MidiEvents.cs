namespace Rector.Midi
{
    public readonly struct MidiNoteEvent
    {
        public int Channel { get; }
        public int NoteNumber { get; }
        public float Velocity { get; }

        public MidiNoteEvent(int channel, int noteNumber, float velocity)
        {
            Channel = channel;
            NoteNumber = noteNumber;
            Velocity = velocity;
        }
    }

    public readonly struct MidiCcEvent
    {
        public int Channel { get; }
        public int ControlNumber { get; }
        public float Value { get; }

        public MidiCcEvent(int channel, int controlNumber, float value)
        {
            Channel = channel;
            ControlNumber = controlNumber;
            Value = value;
        }
    }
}
