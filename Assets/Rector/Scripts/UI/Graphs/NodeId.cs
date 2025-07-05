using System;

namespace Rector.UI.Graphs
{
    public readonly struct NodeId : IEquatable<NodeId>
    {
        public readonly uint Value;

        public NodeId(uint value)
        {
            Value = value;
        }

        public bool Equals(NodeId other)
        {
            return Value == other.Value;
        }

        public override bool Equals(object obj)
        {
            return obj is NodeId other && Equals(other);
        }

        public override int GetHashCode()
        {
            return (int)Value;
        }

        public override string ToString() => $"{Value}";

        static uint currentId;

        public static NodeId Generate()
        {
            return new NodeId(currentId++);
        }

        public static bool operator ==(NodeId left, NodeId right) => left.Equals(right);

        public static bool operator !=(NodeId left, NodeId right) => !left.Equals(right);
    }
}
