using R3;

namespace Rector.UI.Graphs.Nodes
{
    public abstract class Node
    {
        public NodeId Id { get; }
        public string Name { get; }
        public abstract InputSlot[] InputSlots { get; }
        public abstract OutputSlot[] OutputSlots { get; }
        public readonly ReactiveProperty<bool> Selected = new(false);
        public readonly ReactiveProperty<bool> IsMuted = new(false);

        public virtual void DoAction()
        {
        }

        protected Node(NodeId id, string name)
        {
            Id = id;
            Name = name;
        }
    }
}
