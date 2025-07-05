using R3;
using Rector.NodeBehaviours;
using Rector.UI.Graphs.Slots;
using UnityEngine;

namespace Rector.UI.Graphs.Nodes
{
    public sealed class TimeNode : SourceNode
    {
        public const string NodeName = "Time";
        public static NodeCategory GetCategory() => NodeCategory.Event;
        public override NodeCategory Category => GetCategory();

        readonly FloatInput scaleInput = new("scale", 1f, float.NegativeInfinity, float.PositiveInfinity);

        public TimeNode(NodeId id) : base(id, NodeName)
        {
            InputSlots = new[]
            {
                SlotConverter.Convert(id, 0, ActiveInput, IsMuted),
                SlotConverter.Convert(id, 1, scaleInput, IsMuted),
            };

            var scaledTime = 0f;
            var output = new ObservableOutput<float>("time", Observable.EveryUpdate(UnityFrameProvider.Update)
                .Where(_ => IsActive)
                .Select(_ =>
                {
                    scaledTime += Time.deltaTime * scaleInput.Value.Value;
                    return scaledTime;
                }));
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
