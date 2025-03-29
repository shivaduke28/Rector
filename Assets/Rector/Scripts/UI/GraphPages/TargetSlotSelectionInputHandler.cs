using System;
using Rector.UI.Graphs;
using Rector.UI.Graphs.Slots;
using UnityEngine;

namespace Rector.UI.GraphPages
{
    public sealed class TargetSlotSelectionInputHandler : GraphPageInputHandler
    {
        readonly GraphPage graphPage;

        public TargetSlotSelectionInputHandler(GraphPage graphPage)
        {
            this.graphPage = graphPage;
        }


        public override void Navigate(Vector2 value)
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
                graphPage.SetTargetSlot(targetNode.InputSlots[index]);
            }
            else
            {
                var length = targetNode.OutputSlots.Length;
                var index = (targetSlot.Index + direction + length) % length;
                graphPage.SetTargetSlot(targetNode.OutputSlots[index]);
            }
        }

        public override void Cancel()
        {
            graphPage.SetTargetSlot(null);
            graphPage.State.Value = GraphPageState.TargetNodeSelection;
        }

        public override void Submit()
        {
            if (!ToOutputAndInput(graphPage.SelectedSlot.Value, graphPage.TargetSlot.Value, out var output, out var input)) return;
            var edgeId = new EdgeId(output, input);
            if (!graphPage.Graph.RemoveEdge(edgeId))
            {
                if (EdgeConnector.TryConnect(graphPage.SelectedSlot.Value, graphPage.TargetSlot.Value, out var newEdge))
                {
                    graphPage.Graph.AddEdge(newEdge);
                }
            }
        }

        static bool ToOutputAndInput(ISlot slot1, ISlot slot2, out OutputSlot output, out InputSlot input)
        {
            output = slot1.Direction == SlotDirection.Output ? (OutputSlot)slot1 : (OutputSlot)slot2;
            input = slot2.Direction == SlotDirection.Input ? (InputSlot)slot2 : (InputSlot)slot1;
            return output != null && input != null;
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
