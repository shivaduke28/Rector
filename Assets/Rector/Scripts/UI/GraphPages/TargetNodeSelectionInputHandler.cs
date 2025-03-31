using System.Linq;
using Rector.UI.Graphs;
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
            {
                var nextNode = navigator.SelectNextNode(graphPage.TargetNode, value);
                graphPage.SetTargetNode(nextNode);
            }
        }

        public override void Cancel()
        {
            graphPage.SetTargetNode(null);
            graphPage.State.Value = GraphPageState.SlotSelection;
        }

        public override void Submit()
        {
            if (graphPage.SelectedSlot is { } sourceSlot &&
                graphPage.TargetNode is { NodeView: { Node: var targetNode } } &&
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
            graphPage.TargetNode?.NodeView.Node?.DoAction();
        }


        public override void Mute()
        {
            if (graphPage.TargetNode is { NodeView: { Node: var targetNode } })
            {
                targetNode.IsMuted.Value = !targetNode.IsMuted.Value;
            }
        }
    }
}
