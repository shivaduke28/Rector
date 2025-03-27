using System;
using Rector.UI.Nodes;
using UnityEngine;

namespace Rector.UI.Graphs.StateMachine
{
    public sealed class TargetSlotSelectionState : IGraphPageState
    {
        readonly GraphPage graphPage;

        public TargetSlotSelectionState(GraphPage graphPage)
        {
            this.graphPage = graphPage;
        }


        public void Navigate(Vector2 value)
        {
            if (value.x != 0)
            {
                SelectNextTargetSlot(value.x > 0 ? 1 : -1);
            }
        }

        void SelectNextTargetSlot(int direction)
        {
            if (graphPage.TargetNode.Value is not { } targetNode) throw new InvalidOperationException("target node is null");
            if (graphPage.TargetSlot.Value is not { } targetSlot) throw new InvalidOperationException("target slot is null");

            if (targetSlot.Direction == SlotDirection.Input)
            {
                var length = targetNode.InputSlots.Length;
                var index = (targetSlot.Index + direction + length) % length;
                graphPage.SelectTargetSlot(targetNode.InputSlots[index]);
            }
            else
            {
                var length = targetNode.OutputSlots.Length;
                var index = (targetSlot.Index + direction + length) % length;
                graphPage.SelectTargetSlot(targetNode.OutputSlots[index]);
            }
        }

        public void Cancel()
        {
            graphPage.SelectTargetSlot(null);
            graphPage.State.Value = GraphPageState.TargetNodeSelection;
        }

        public void Submit()
        {
            if (!ToOutputAndInput(graphPage.SelectedSlot.Value, graphPage.TargetSlot.Value, out var output, out var input)) return;
            var edgeId = new EdgeId(output, input);
            if (graphPage.Graph.TryGet(edgeId, out _))
            {
                graphPage.Graph.Remove(edgeId);
            }
            else
            {
                if (EdgeConnector.TryConnect(graphPage.SelectedSlot.Value, graphPage.TargetSlot.Value, out var newEdge))
                {
                    graphPage.Graph.Add(newEdge);
                }
            }
        }

        static bool ToOutputAndInput(ISlot slot1, ISlot slot2, out OutputSlot output, out InputSlot input)
        {
            output = slot1.Direction == SlotDirection.Output ? (OutputSlot)slot1 : (OutputSlot)slot2;
            input = slot2.Direction == SlotDirection.Input ? (InputSlot)slot2 : (InputSlot)slot1;
            return output != null && input != null;
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
