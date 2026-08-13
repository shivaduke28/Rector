using R3;
using Rector.UI.Graphs.Slots;
using UnityEngine;

namespace Rector.UI.Graphs.Nodes
{
    public sealed class LoopNode : Node
    {
        public const string NodeName = "Loop";
        public static NodeCategory GetCategory() => NodeCategory.Operator;
        public override NodeCategory Category => GetCategory();
        public override InputSlot[] InputSlots { get; }
        public override OutputSlot[] OutputSlots { get; }

        // 同値の拍（length=1のSeqなど）でも毎回評価が走るよう、同値通知を抑制しない
        readonly ReactiveProperty<int> beat = new(0, equalityComparer: null);
        readonly ReactiveProperty<int> length = new(4);

        public LoopNode(NodeId id) : base(id, NodeName)
        {
            var beatSlot = new ReactivePropertyIntInputSlot(id, 0, "Beat", beat, 0, 0, 256, IsMuted);
            InputSlots = new InputSlot[]
            {
                beatSlot,
                new ReactivePropertyIntInputSlot(id, 1, "Length", length, 4, 1, 256, IsMuted)
            };

            // 未接続時は評価しない: 最後のエッジが外れるとスロットがdefault(0)にリセットされ、
            // null comparerだとそれが発火して偽のOnが飛ぶため
            var beats = beat.Where(_ => beatSlot.ConnectedCount > 0);

            // On/OffはDistinctUntilChangedを付けずに毎拍emitする
            // （付けるとlength=1のとき値が変化せず一度も発火しなくなる）
            OutputSlots = new OutputSlot[]
            {
                new ObservableOutputSlot<bool>(id, 0, "On", beats.Select(b => b % Len() == 0), IsMuted),
                new ObservableOutputSlot<bool>(id, 1, "Off", beats.Select(b => b % Len() != 0), IsMuted),
                new ObservableOutputSlot<int>(id, 2, "Phase", beats.Select(b => b % Len()), IsMuted),
                new ObservableOutputSlot<int>(id, 3, "Cycle", beats.Select(b => b / Len()), IsMuted)
            };
        }

        int Len() => Mathf.Max(1, length.Value);
    }
}
