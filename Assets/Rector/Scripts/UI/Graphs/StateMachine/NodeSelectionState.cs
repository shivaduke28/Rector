using System.Linq;
using UnityEngine;

namespace Rector.UI.Graphs.StateMachine
{
    public sealed class NodeSelectionState : IGraphPageState
    {
        readonly GraphPage graphPage;

        public NodeSelectionState(GraphPage graphPage)
        {
            this.graphPage = graphPage;
        }

        public void Navigate(Vector2 value)
        {
            if (value.sqrMagnitude == 0f) return;
            if (graphPage.SelectedNode.Value is { } selectedNode && graphPage.NodeViews.TryGetValue(selectedNode.Id, out var selectedNodeView))
            {
                var nextNodeView = NodeNavigator.SelectNextNode(selectedNodeView, value, graphPage.Layers);
                if (nextNodeView != null)
                {
                    graphPage.MoveGraphContentToNodeVisible(nextNodeView);
                    graphPage.SelectNode(nextNodeView.Node);
                }
            }
            else
            {
                var first = graphPage.Layers.FirstOrDefault()?.FirstOrDefault()?.Node;
                if (first != null)
                {
                    graphPage.SelectNode(first);
                }
            }
        }

        public void Cancel()
        {
        }

        public void Submit()
        {
            if (graphPage.SelectedNode.Value is { } selected && (selected.InputSlots.Length > 0 || selected.OutputSlots.Length > 0))
            {
                graphPage.State.Value = GraphPageState.SlotSelection;
                graphPage.SelectSlot(selected.InputSlots.Length > 0 ? selected.InputSlots[0] : selected.OutputSlots[0]);
            }
        }

        public void Action1()
        {
            graphPage.SelectedNode.Value?.DoAction();
        }

        public void Action2()
        {
            graphPage.State.Value = GraphPageState.NodeCreation;
        }

        public void SubmitHoldStart()
        {
            if (graphPage.SelectedNode.Value is { } selectedNode && graphPage.NodeViews.TryGetValue(selectedNode.Id, out var selectedNodeView))
            {
                graphPage.ShowHoldNextTo(selectedNodeView);
            }
        }

        public void SubmitHoldCancel()
        {
            graphPage.HideHold();
        }

        public void SubmitHold()
        {
            graphPage.HideHold();
            if (graphPage.SelectedNode.Value is { } selectedNode)
            {
                graphPage.Graph.RemoveEdgesFrom(selectedNode);
            }
        }

        public void Action2HoldStart()
        {
            if (graphPage.SelectedNode.Value is { } selectedNode && graphPage.NodeViews.TryGetValue(selectedNode.Id, out var selectedNodeView))
            {
                graphPage.ShowHoldNextTo(selectedNodeView);
            }
        }

        public void Action2HoldCancel()
        {
            graphPage.HideHold();
        }

        public void Action2Hold()
        {
            graphPage.HideHold();
            graphPage.RemoveSelectedNode();
        }

        public void ToggleMute()
        {
            if (graphPage.SelectedNode.Value is { } selectedNode)
            {
                var mute = !selectedNode.IsMuted.Value;
                selectedNode.IsMuted.Value = mute;
                RectorLogger.ToggleMute(selectedNode, mute);
            }
        }
    }
}
