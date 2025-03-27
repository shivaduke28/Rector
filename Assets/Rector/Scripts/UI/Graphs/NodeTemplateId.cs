using System;

namespace Rector.UI.Graphs
{
    public readonly struct NodeTemplateId : IEquatable<NodeTemplateId>
    {
        public readonly uint Value;

        public NodeTemplateId(uint value)
        {
            Value = value;
        }

        public bool Equals(NodeTemplateId other)
        {
            return Value == other.Value;
        }

        public override bool Equals(object obj)
        {
            return obj is NodeTemplateId other && Equals(other);
        }

        public override int GetHashCode()
        {
            return (int) Value;
        }

        public static bool operator ==(NodeTemplateId left, NodeTemplateId right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(NodeTemplateId left, NodeTemplateId right)
        {
            return !left.Equals(right);
        }


        static uint currentId;

        public static NodeTemplateId Generate()
        {
            return new NodeTemplateId(currentId++);
        }
    }
}
