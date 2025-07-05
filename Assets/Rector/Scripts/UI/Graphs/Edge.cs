using System;
using Rector.UI.Graphs.Nodes;
using Rector.UI.Graphs.Slots;
using UnityEngine.Assertions;

namespace Rector.UI.Graphs
{
    public readonly struct EdgeId : IEquatable<EdgeId>
    {
        public readonly NodeId OutputNodeId;
        public readonly int OutputSlotIndex;
        public readonly NodeId InputNodeId;
        public readonly int InputSlotIndex;

        public EdgeId(OutputSlot outputSlot, InputSlot inputSlot)
        {
            OutputNodeId = outputSlot.NodeId;
            OutputSlotIndex = outputSlot.Index;
            InputNodeId = inputSlot.NodeId;
            InputSlotIndex = inputSlot.Index;
        }

        public EdgeId(NodeId outputNodeId, int outputSlotIndex, NodeId inputNodeId, int inputSlotIndex)
        {
            OutputNodeId = outputNodeId;
            OutputSlotIndex = outputSlotIndex;
            InputNodeId = inputNodeId;
            InputSlotIndex = inputSlotIndex;
        }

        public bool Equals(EdgeId other)
        {
            return OutputNodeId.Equals(other.OutputNodeId) &&
                   OutputSlotIndex == other.OutputSlotIndex &&
                   InputNodeId.Equals(other.InputNodeId) &&
                   InputSlotIndex == other.InputSlotIndex;
        }

        public override bool Equals(object obj)
        {
            return obj is EdgeId other && Equals(other);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(OutputNodeId, OutputSlotIndex, InputNodeId, InputSlotIndex);
        }

        public override string ToString()
        {
            return $"EdgeId:{OutputNodeId}.{OutputSlotIndex}->{InputNodeId}.{InputSlotIndex}";
        }
    }

    public sealed class Edge : IDisposable
    {
        public readonly EdgeId Id;
        public readonly OutputSlot OutputSlot;
        public readonly InputSlot InputSlot;
        readonly IDisposable disposable;

        public Edge(OutputSlot outputSlot, InputSlot inputSlot, IDisposable disposable)
        {
            Id = new EdgeId(outputSlot, inputSlot);
            Assert.IsTrue(outputSlot.Direction == SlotDirection.Output);
            Assert.IsTrue(inputSlot.Direction == SlotDirection.Input);
            OutputSlot = outputSlot;
            InputSlot = inputSlot;
            // ctorで呼ぶのちょっと渋い気もするがDisconnectedと同じ場所で呼びたい
            InputSlot.OnConnected();
            OutputSlot.OnConnected();
            this.disposable = disposable;
        }

        public bool IsConnectedTo(ISlot slot) => OutputSlot == slot || InputSlot == slot;
        public bool IsConnectedTo(Node node) => OutputSlot.NodeId.Equals(node.Id) || InputSlot.NodeId.Equals(node.Id);

        public void Dispose()
        {
            disposable.Dispose();
            InputSlot.Disconnected();
            OutputSlot.Disconnected();
        }
    }
}
