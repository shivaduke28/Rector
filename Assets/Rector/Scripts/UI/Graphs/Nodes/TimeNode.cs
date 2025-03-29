using System;
using R3;
using Rector.Nodes;
using UnityEngine;

namespace Rector.UI.Graphs.Nodes
{
    public sealed class TimeNode : Node
    {
        public const string NodeName = "Time";
        public static string Category => NodeCategoryV2.Event;

        public TimeNode(NodeId id) : base(id, NodeName)
        {
            var output = new ObservableOutput<float>("time", Observable.EveryUpdate(UnityFrameProvider.Update).Select(_ => Time.time));
            var timeFraction = new ObservableOutput<float>("frac", output.Observable.Select(t => t % 1));
            OutputSlots = new[]
            {
                SlotConverter.Convert(id, 0, output, IsMuted),
                SlotConverter.Convert(id, 1, timeFraction, IsMuted)
            };
        }

        public override InputSlot[] InputSlots { get; } = Array.Empty<InputSlot>();
        public override OutputSlot[] OutputSlots { get; }
    }
}
