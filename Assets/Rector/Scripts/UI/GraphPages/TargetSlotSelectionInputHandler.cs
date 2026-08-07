using System;
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
            if (graphPage.TargetNode is not { NodeView: { Node: var targetNode } }) throw new InvalidOperationException("target node is null");
            if (graphPage.TargetSlot is not { } targetSlot) throw new InvalidOperationException("target slot is null");

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

        // 繋がっていれば外す、繋がっていなければ繋ぐトグル。検証と接続そのものは GraphPage 側に
        // あり、CLI も同じものを呼ぶ。
        public override void Submit()
        {
            if (!ToOutputAndInput(graphPage.SelectedSlot, graphPage.TargetSlot, out var output, out var input)) return;
            if (graphPage.DisconnectSlots(output, input)) return;

            // OpenNodeParameter を押しながらの接続は、選択中スロットの既存エッジを差し替える
            graphPage.TryConnectSlots(output, input, graphPage.IsNodeParameterOpen ? graphPage.SelectedSlot : null);
        }

        static bool ToOutputAndInput(ISlot slot1, ISlot slot2, out OutputSlot output, out InputSlot input)
        {
            if (slot1 == null || slot2 == null)
            {
                output = null;
                input = null;
                return false;
            }

            output = slot1.Direction == SlotDirection.Output ? (OutputSlot)slot1 : (OutputSlot)slot2;
            input = slot2.Direction == SlotDirection.Input ? (InputSlot)slot2 : (InputSlot)slot1;
            return output != null && input != null;
        }


        public override void Action()
        {
            graphPage.TargetNode?.NodeView.Node.DoAction();
        }

        public override void AddNode()
        {
            graphPage.PromoteTargetToSource();
        }

        public override void Mute()
        {
            graphPage.ToggleMute(graphPage.TargetNode);
        }
    }
}
