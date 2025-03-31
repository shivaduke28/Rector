﻿using System;
using R3;
using Rector.UI.Graphs.Slots;
using UnityEngine;

namespace Rector.UI.GraphPages
{
    public sealed class SlotSelectionInputHandler : GraphPageInputHandler
    {
        readonly GraphPage graphPage;

        public SlotSelectionInputHandler(GraphPage graphPage)
        {
            this.graphPage = graphPage;
        }

        public override void Navigate(Vector2 value)
        {
            if (value.sqrMagnitude == 0f) return;
            if (value.x != 0)
            {
                SelectNextSlot(value.x > 0 ? 1 : -1);
            }
            else if (value.y != 0)
            {
                SelectOppositeSlot();
            }
        }

        void SelectNextSlot(int direction)
        {
            if (graphPage.SelectedNode is not { NodeView: { Node: var selectedNode } }) throw new InvalidOperationException("selected node is null");
            if (graphPage.SelectedSlot is not { } selectedSlot) throw new InvalidOperationException("selected slot is null");

            if (selectedSlot.Direction == SlotDirection.Input)
            {
                var length = selectedNode.InputSlots.Length;
                var index = (selectedSlot.Index + direction + length) % length;
                graphPage.SelectSlot(selectedNode.InputSlots[index]);
            }
            else
            {
                var length = selectedNode.OutputSlots.Length;
                var index = (selectedSlot.Index + direction + length) % length;
                graphPage.SelectSlot(selectedNode.OutputSlots[index]);
            }
        }

        void SelectOppositeSlot()
        {
            if (graphPage.SelectedNode is not { NodeView: { Node: var selectedNode } }) throw new InvalidOperationException("selected node is null");
            if (graphPage.SelectedSlot is not { } selectedSlot) throw new InvalidOperationException("selected slot is null");

            if (selectedSlot.Direction == SlotDirection.Input)
            {
                if (selectedNode.OutputSlots.Length == 0) return;
                var t = selectedSlot.Index / (float)selectedNode.InputSlots.Length;
                var index = (int)(t * selectedNode.OutputSlots.Length);
                graphPage.SelectSlot(selectedNode.OutputSlots[index]);
            }
            else
            {
                if (selectedNode.InputSlots.Length == 0) return;
                var t = selectedSlot.Index / (float)selectedNode.OutputSlots.Length;
                var index = (int)(t * selectedNode.InputSlots.Length);
                graphPage.SelectSlot(selectedNode.InputSlots[index]);
            }
        }


        public override void Cancel()
        {
            if (graphPage.State.Value != GraphPageState.SlotSelection) throw new InvalidOperationException($"state is not {GraphPageState.SlotSelection}");
            graphPage.SelectSlot(null);
            graphPage.State.Value = GraphPageState.NodeSelection;
        }

        public override void Submit()
        {
            graphPage.TargetSlot = null;
            graphPage.TargetNode = graphPage.SelectedNode;
            graphPage.State.Value = GraphPageState.TargetNodeSelection;
        }

        public override void Action()
        {
            // ちょっと難しすぎるかも
            switch (graphPage.SelectedSlot)
            {
                case CallbackInputSlot callbackInputSlot:
                    callbackInputSlot.Send(Unit.Default);
                    break;
                case ReactivePropertyInputSlot<bool> boolInputSlot:
                    boolInputSlot.Send(!boolInputSlot.Property.Value);
                    break;
            }
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
                    if (graphPage.SelectedSlot is { } selectedSlot)
                    {
                        graphPage.Graph.RemoveEdgesFrom(selectedSlot);
                        graphPage.Sort();
                    }

                    break;
                }
                default:
                    throw new ArgumentOutOfRangeException(nameof(state), state, null);
            }
        }

        public override void Mute()
        {
            if (graphPage.SelectedNode is { NodeView: { Node : var selectedNode } })
            {
                selectedNode.IsMuted.Value = !selectedNode.IsMuted.Value;
            }
        }
    }
}
