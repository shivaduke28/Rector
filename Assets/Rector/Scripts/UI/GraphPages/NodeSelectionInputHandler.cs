using System;
using System.Linq;
using Rector.UI.LayeredGraphDrawing;
using UnityEngine;

namespace Rector.UI.GraphPages
{
    public sealed class NodeSelectionInputHandler : GraphPageInputHandler
    {
        readonly GraphPage graphPage;
        readonly NodeNavigator nodeNavigator;

        public NodeSelectionInputHandler(GraphPage graphPage, NodeNavigator nodeNavigator)
        {
            this.graphPage = graphPage;
            this.nodeNavigator = nodeNavigator;
        }

        public override void Navigate(Vector2 value)
        {
            if (value.sqrMagnitude == 0f) return;
            if (graphPage.SelectedNode.Value is { } selectedNode && graphPage.Graph.TryGetNode(selectedNode.Id, out var selectedNodeView))
            {
                var nextNodeView = nodeNavigator.SelectNextNode(selectedNodeView, value);
                graphPage.SelectNode(nextNodeView.NodeView.Node);
            }
            else
            {
                var first = graphPage.Graph.Layers.FirstOrDefault(l => l.Count > 0)?.FirstOrDefault();
                if (first is LayeredNode layeredNode)
                {
                    graphPage.SelectNode(layeredNode.NodeView.Node);
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
                    graphPage.ShowHoldNextToSelected();
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
                        graphPage.Sort();
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
                    graphPage.ShowHoldNextToSelected();
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
                graphPage.State.Value = GraphPageState.NodeParameter;
            }
        }
    }
}
