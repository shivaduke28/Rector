using System;
using System.Linq;
using UnityEngine;

namespace Rector.UI.Graphs.StateMachine
{
    public sealed class NodeSelectionInputHandler : GraphPageInputHandler
    {
        readonly GraphPage graphPage;

        public NodeSelectionInputHandler(GraphPage graphPage)
        {
            this.graphPage = graphPage;
        }

        public override void Navigate(Vector2 value)
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

        public override void Submit()
        {
            if (graphPage.SelectedNode.Value is { } selected && (selected.InputSlots.Length > 0 || selected.OutputSlots.Length > 0))
            {
                graphPage.State.Value = GraphPageState.SlotSelection;
                graphPage.SelectSlot(selected.InputSlots.Length > 0 ? selected.InputSlots[0] : selected.OutputSlots[0]);
            }
        }

        public override void Action()
        {
            graphPage.SelectedNode.Value?.DoAction();
        }

        public override void AddNode()
        {
            graphPage.State.Value = GraphPageState.NodeCreation;
        }

        public override void RemoveEdge(HoldState state)
        {
            switch (state)
            {
                case HoldState.Start:
                {
                    if (graphPage.SelectedNode.Value is { } selectedNode && graphPage.NodeViews.TryGetValue(selectedNode.Id, out var selectedNodeView))
                    {
                        graphPage.ShowHoldNextTo(selectedNodeView);
                    }

                    break;
                }
                case HoldState.Cancel:
                    graphPage.HideHold();
                    break;
                case HoldState.Perform:
                {
                    graphPage.HideHold();
                    if (graphPage.SelectedNode.Value is { } selectedNode)
                    {
                        graphPage.Graph.RemoveEdgesFrom(selectedNode);
                    }

                    break;
                }
                default:
                    throw new ArgumentOutOfRangeException(nameof(state), state, null);
            }
        }

        public override void RemoveNode(HoldState state)
        {
            switch (state)
            {
                case HoldState.Start:
                    if (graphPage.SelectedNode.Value is { } selectedNode && graphPage.NodeViews.TryGetValue(selectedNode.Id, out var selectedNodeView))
                    {
                        graphPage.ShowHoldNextTo(selectedNodeView);
                    }

                    break;
                case HoldState.Cancel:
                    graphPage.HideHold();
                    break;
                case HoldState.Perform:
                    graphPage.HideHold();
                    graphPage.RemoveSelectedNode();

                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(state), state, null);
            }
        }


        public override void Mute()
        {
            if (graphPage.SelectedNode.Value is { } selectedNode)
            {
                var mute = !selectedNode.IsMuted.Value;
                selectedNode.IsMuted.Value = mute;
                RectorLogger.ToggleMute(selectedNode, mute);
            }
        }

        public override void OpenNodeParameter()
        {
            if (graphPage.SelectedNode.Value is not null)
            {
                graphPage.State.Value = GraphPageState.NodeDetail;
            }
        }
    }
}
