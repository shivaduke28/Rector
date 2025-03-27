using System.Linq;
using Rector.UI.Nodes;
using UnityEngine;

namespace Rector.UI.Graphs.StateMachine
{
    public sealed class TargetNodeSelectionState : IGraphPageState
    {
        readonly GraphPage graphPage;

        public TargetNodeSelectionState(GraphPage graphPage)
        {
            this.graphPage = graphPage;
        }

        public void Navigate(Vector2 value)
        {
            if (value.sqrMagnitude == 0f) return;
            if (graphPage.NodeViews.TryGetValue(graphPage.TargetNode.Value.Id, out var targetNodeView))
            {
                var nextNodeView = NodeNavigator.SelectNextNode(targetNodeView, value, graphPage.Layers);
                if (nextNodeView != null)
                {
                    graphPage.MoveGraphContentToNodeVisible(nextNodeView);
                    graphPage.SelectTargetNode(nextNodeView.Node);
                }
            }
        }

        public void Cancel()
        {
            graphPage.SelectTargetNode(null);
            graphPage.State.Value = GraphPageState.SlotSelection;
        }

        public void Submit()
        {
            if (graphPage.SelectedSlot.Value is { } sourceSlot && graphPage.TargetNode.Value is { } targetNode &&
                EdgeConnector.CanConnect(sourceSlot, targetNode))
            {
                graphPage.State.Value = GraphPageState.TargetSlotSelection;
                if (sourceSlot.Direction == SlotDirection.Output)
                {
                    graphPage.SelectTargetSlot(targetNode.InputSlots.FirstOrDefault(x => EdgeConnector.CanConnect(sourceSlot, x)));
                }
                else
                {
                    graphPage.SelectTargetSlot(targetNode.OutputSlots.FirstOrDefault(x => EdgeConnector.CanConnect(x, sourceSlot)));
                }
            }
        }

        public void Action1()
        {
            graphPage.TargetNode.Value?.DoAction();
        }

        public void Action2()
        {
        }

        public void SubmitHoldStart()
        {
        }

        public void SubmitHoldCancel()
        {
        }

        public void SubmitHold()
        {
        }

        public void Action2HoldStart()
        {
        }

        public void Action2HoldCancel()
        {
        }

        public void Action2Hold()
        {
        }

        public void ToggleMute()
        {
            if (graphPage.TargetNode.Value is { } targetNode)
            {
                targetNode.IsMuted.Value = !targetNode.IsMuted.Value;
            }
        }
    }
}
