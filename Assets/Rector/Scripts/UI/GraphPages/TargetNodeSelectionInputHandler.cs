using System.Linq;
using Rector.UI.Graphs;
using Rector.UI.Graphs.Nodes;
using Rector.UI.Graphs.Slots;
using UnityEngine;

namespace Rector.UI.GraphPages
{
    public sealed class TargetNodeSelectionInputHandler : GraphPageInputHandler
    {
        readonly GraphPage graphPage;
        readonly NodeNavigator navigator;

        public TargetNodeSelectionInputHandler(GraphPage graphPage, NodeNavigator navigator)
        {
            this.graphPage = graphPage;
            this.navigator = navigator;
        }

        public override void Navigate(Vector2 value)
        {
            if (value.sqrMagnitude == 0f) return;
            if (graphPage.Graph.TryGetNode(graphPage.TargetNode.Value.Id, out var targetLayeredNode))
            {
                var nextNode = navigator.SelectNextNode(targetLayeredNode, value);
                graphPage.SetTargetNode(nextNode.NodeView.Node);
            }
        }

        public override void Cancel()
        {
            graphPage.SetTargetNode(null);
            graphPage.State.Value = GraphPageState.SlotSelection;
        }

        public override void Submit()
        {
            if (graphPage.SelectedSlot.Value is { } sourceSlot && graphPage.TargetNode.Value is { } targetNode &&
                EdgeConnector.CanConnect(sourceSlot, targetNode))
            {
                graphPage.State.Value = GraphPageState.TargetSlotSelection;
                if (sourceSlot.Direction == SlotDirection.Output)
                {
                    graphPage.SetTargetSlot(targetNode.InputSlots.FirstOrDefault(x => EdgeConnector.CanConnect(sourceSlot, x)));
                }
                else
                {
                    graphPage.SetTargetSlot(targetNode.OutputSlots.FirstOrDefault(x => EdgeConnector.CanConnect(x, sourceSlot)));
                }
            }
        }

        public override void Action()
        {
            graphPage.TargetNode.Value?.DoAction();
        }


        public override void Mute()
        {
            if (graphPage.TargetNode.Value is { } targetNode)
            {
                targetNode.IsMuted.Value = !targetNode.IsMuted.Value;
            }
        }
    }
}
