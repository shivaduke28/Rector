using R3;
using Rector.UI.Graphs.Slots;

namespace Rector.UI.Graphs.Nodes
{
    public sealed class GateNode : SourceNode
    {
        public const string NodeName = "Gate";
        public static string Category => NodeCategory.Operator;
        readonly Subject<Unit> subject = new();

        public GateNode(NodeId id) : base(id, NodeName)
        {
            InputSlots = new InputSlot[]
            {
                new CallbackInputSlot(id, 0, "In", () => subject.OnNext(Unit.Default), IsMuted),
                new ReactivePropertyInputSlot<bool>(id, 1, "Gate", IsActive, IsActive.Value, IsMuted),
            };

            OutputSlots = new OutputSlot[]
            {
                new ObservableOutputSlot<Unit>(id, 0, "Out", subject.Where(_ => IsActive.Value), IsMuted)
            };
        }

        public override InputSlot[] InputSlots { get; }
        public override OutputSlot[] OutputSlots { get; }
    }
}
