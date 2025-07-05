using Rector.NodeBehaviours;

namespace Rector.UI.Graphs.Nodes
{
    public abstract class SourceNode : Node
    {
        protected readonly BoolInput ActiveInput;
        protected bool IsActive => ActiveInput.Value.Value;
        public sealed override void DoAction() => ActiveInput.Value.Value = !ActiveInput.Value.Value;

        protected SourceNode(NodeId id, string name, bool defaultValue = true) : base(id, name)
        {
            ActiveInput = new BoolInput("Active", defaultValue);
        }
    }
}
