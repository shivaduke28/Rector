using R3;
using Rector.UI.Graphs.Slots;

namespace Rector.UI.Graphs.Nodes
{
    public sealed class GateNode : Node
    {
        public const string NodeName = "Gate";
        public static NodeCategory GetCategory() => NodeCategory.Operator;
        public override NodeCategory Category => GetCategory();
        readonly Subject<float> subject = new();
        readonly ReactiveProperty<bool> gate = new(true);

        public GateNode(NodeId id) : base(id, NodeName)
        {
            InputSlots = new InputSlot[]
            {
                new CallbackFloatInputSlot(id, 0, "In", x => subject.OnNext(x), float.NegativeInfinity, float.PositiveInfinity, IsMuted),
                new ReactivePropertyInputSlot<bool>(id, 1, "Gate", gate, gate.Value, IsMuted),
            };

            OutputSlots = new OutputSlot[]
            {
                new ObservableOutputSlot<float>(id, 0, "Out", subject.Where(_ => gate.Value), IsMuted)
            };
        }

        public override void DoAction()
        {
            gate.Value = !gate.Value;
        }

        public override InputSlot[] InputSlots { get; }
        public override OutputSlot[] OutputSlots { get; }
    }
}
