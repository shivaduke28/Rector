using System;
using R3;
using Rector.UI.Graphs.Slots;

namespace Rector.UI.Graphs.Nodes
{
    public sealed class ButtonNode : Node
    {
        public static string Category => NodeCategory.Event;

        public const string NodeName = "Button";
        readonly Subject<Unit> onClick = new();
        public ButtonNode(NodeId id) : base(id, "Button")
        {
            OutputSlots = new OutputSlot[]
            {
                new ObservableOutputSlot<Unit>(id, 0, "Pressed", onClick, IsMuted)
            };
        }

        public override void DoAction()
        {
            onClick.OnNext(Unit.Default);
        }

        public override InputSlot[] InputSlots { get; } = Array.Empty<InputSlot>();
        public override OutputSlot[] OutputSlots { get; }
    }
}
