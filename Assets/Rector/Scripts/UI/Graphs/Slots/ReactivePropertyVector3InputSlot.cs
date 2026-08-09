using R3;
using UnityEngine;

namespace Rector.UI.Graphs.Slots
{
    public sealed class ReactivePropertyVector3InputSlot : ReactivePropertyInputSlot<Vector3>
    {
        // レンジはX/Y/Zで共通。範囲を持たない入力では ±Infinity が入る。
        public readonly float MinValue;
        public readonly float MaxValue;

        public ReactivePropertyVector3InputSlot(NodeId nodeId, int index, string name, ReactiveProperty<Vector3> property,
            Vector3 defaultValue, float minValue, float maxValue, ReadOnlyReactiveProperty<bool> isMuted) : base(nodeId, index, name, property, defaultValue, isMuted)
        {
            MinValue = minValue;
            MaxValue = maxValue;
        }
    }
}
