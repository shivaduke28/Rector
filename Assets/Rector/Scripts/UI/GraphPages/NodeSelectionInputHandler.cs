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
            if (graphPage.SelectedNode is { } selectedNode)
            {
                var nextNodeView = nodeNavigator.SelectNextNode(selectedNode, value);
                graphPage.SelectNode(nextNodeView);
            }
            else
            {
                var first = graphPage.Graph.Layers.FirstOrDefault(l => l.Count > 0)?.FirstOrDefault();
                if (first is LayeredNode layeredNode)
                {
                    graphPage.SelectNode(layeredNode);
                }
            }
        }

        public override void MoveNodeToGroup(int direction)
        {
            graphPage.MoveSelectedNodeToGroup(direction);
        }

        public override void Submit()
        {
            if (graphPage.SelectedNode is { } selected && (selected.InputSlotCount > 0 || selected.OutputSlotCount > 0))
            {
                graphPage.State.Value = GraphPageState.SlotSelection;
                graphPage.SelectSlot(selected.InputSlotCount > 0 ? selected.NodeView.Node.InputSlots[0] : selected.NodeView.Node.OutputSlots[0]);
            }
        }

        public override void Action()
        {
            graphPage.SelectedNode?.NodeView.Node.DoAction();
        }

        public override void AddNode()
        {
            // R2(ALT)で掴んでいる間の△(C)はコピー。未選択でも作成メニューへは倒さない。
            // R2押下中はd-padがグループ移動に取られてメニューが操作不能になるため
            if (graphPage.IsGrabbing)
            {
                graphPage.CopySelectedNode();
                return;
            }

            graphPage.State.Value = GraphPageState.NodeCreation;
        }

        public override void RemoveEdge(HoldState state)
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
                    if (graphPage.SelectedNode is { } selectedNode)
                    {
                        graphPage.Graph.RemoveEdgesFrom(selectedNode);
                        graphPage.Sort();
                    }

                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(state), state, null);
            }
        }

        public override void RemoveNode(HoldState state)
        {
            // 掴んでいる間の△はコピーなので、握りすぎてホールドが成立しても削除しない
            switch (state)
            {
                case HoldState.Start:
                    if (graphPage.IsGrabbing) break;
                    graphPage.ShowHoldNextToSelected();
                    break;
                case HoldState.Cancel:
                    graphPage.HideHold();
                    break;
                case HoldState.Perform:
                    // ガイドは無条件に消す。非Grabで押し始めてからR2を握った場合の残留を防ぐ
                    graphPage.HideHold();
                    if (graphPage.IsGrabbing) break;
                    graphPage.RemoveSelectedNode();

                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(state), state, null);
            }
        }


        public override void Mute()
        {
            graphPage.ToggleMute(graphPage.SelectedNode);
        }

        public override void OpenNodeParameter()
        {
            if (graphPage.SelectedNode is not null)
            {
                graphPage.State.Value = GraphPageState.NodeParameter;
            }
        }
    }
}
