using R3;
using Rector.UI.Graphs.Slots;

namespace Rector.UI.Graphs.Nodes
{
    public sealed class GateNode : Node
    {
        public const string NodeName = "Gate";
        public static string Category => NodeCategory.Operator;
        readonly Subject<Unit> subject = new();
        readonly ReactiveProperty<bool> gate = new(true);

        public GateNode(NodeId id) : base(id, NodeName)
        {
            InputSlots = new InputSlot[]
            {
                new CallbackInputSlot(id, 0, "In", () => subject.OnNext(Unit.Default), IsMuted),
                new ReactivePropertyInputSlot<bool>(id, 1, "Gate", gate, gate.Value, IsMuted),
            };

            OutputSlots = new OutputSlot[]
            {
                new ObservableOutputSlot<Unit>(id, 0, "Out", subject.Where(_ => gate.Value), IsMuted)
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
