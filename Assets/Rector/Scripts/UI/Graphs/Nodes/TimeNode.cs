using R3;
using Rector.Nodes;
using Rector.UI.Graphs.Slots;
using UnityEngine;

namespace Rector.UI.Graphs.Nodes
{
    public sealed class TimeNode : SourceNode
    {
        public const string NodeName = "Time";
        public static string Category => NodeCategory.Event;

        public TimeNode(NodeId id) : base(id, NodeName)
        {
            InputSlots = new[]
            {
                SlotConverter.Convert(id, 0, ActiveInput, IsMuted),
            };

            var output = new ObservableOutput<float>("time", Observable.EveryUpdate(UnityFrameProvider.Update).Where(_ => IsActive).Select(_ => Time.time));
            var timeFraction = new ObservableOutput<float>("frac", output.Observable.Where(_ => IsActive).Select(t => t % 1));
            OutputSlots = new[]
            {
                SlotConverter.Convert(id, 0, output, IsMuted),
                SlotConverter.Convert(id, 1, timeFraction, IsMuted)
            };
        }

        public override InputSlot[] InputSlots { get; }
        public override OutputSlot[] OutputSlots { get; }
    }
}
