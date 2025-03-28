﻿using R3;

namespace Rector.UI.Graphs.Nodes
{
    public abstract class SourceNode : Node
    {
        public readonly ReactiveProperty<bool> IsActive;
        public sealed override void DoAction() => IsActive.Value = !IsActive.Value;

        protected SourceNode(NodeId id, string name, bool defaultValue = true) : base(id, name)
        {
            IsActive = new ReactiveProperty<bool>(defaultValue);
        }
    }
}
