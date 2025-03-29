using UnityEngine;

namespace Rector.UI.GraphPages
{
    public sealed class NodeCreationInputHandler : GraphPageInputHandler
    {
        readonly CreateNodeMenuView createNodeMenuView;

        public NodeCreationInputHandler(CreateNodeMenuView createNodeMenuView)
        {
            this.createNodeMenuView = createNodeMenuView;
        }

        public override void Navigate(Vector2 value) => createNodeMenuView.Navigate(value);
        public override void Submit() => createNodeMenuView.Submit();
        public override void Cancel() => createNodeMenuView.Cancel();
    }
}
