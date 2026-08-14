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

        // Beat は1始まりの位置 (Seq.Beat, Loop.Cycle など)。0は「位置なし」。
        // 同値の拍（length=1のSeqなど）でも毎回評価が走るよう、同値通知を抑制しない
        readonly ReactiveProperty<int> beat = new(0, equalityComparer: null);
        readonly ReactiveProperty<int> length = new(4);

        public ReadOnlyReactiveProperty<int> Beat => beat;
        public ReadOnlyReactiveProperty<int> Length => length;

        public LoopNode(NodeId id) : base(id, NodeName)
        {
            InputSlots = new InputSlot[]
            {
                new ReactivePropertyIntInputSlot(id, 0, "Beat", beat, 0, 0, 256, IsMuted),
                new ReactivePropertyIntInputSlot(id, 1, "Length", length, 4, 1, 256, IsMuted)
            };

            // 1未満は位置ではないので評価しない。未接続時のdefault(0)や、
            // エッジ切断時に0へリセットされたときの偽発火もここで止まる
            var beats = beat.Where(b => b >= 1);

            // On/OffはDistinctUntilChangedを付けずに毎拍emitする
            // （付けるとlength=1のとき値が変化せず一度も発火しなくなる）
            OutputSlots = new OutputSlot[]
            {
                new ObservableOutputSlot<bool>(id, 0, "On", beats.Select(b => (b - 1) % Len() == 0), IsMuted),
                new ObservableOutputSlot<bool>(id, 1, "Off", beats.Select(b => (b - 1) % Len() != 0), IsMuted),
                new ObservableOutputSlot<int>(id, 2, "Phase", beats.Select(b => (b - 1) % Len() + 1), IsMuted),
                new ObservableOutputSlot<int>(id, 3, "Cycle", beats.Select(b => (b - 1) / Len() + 1), IsMuted)
            };
        }

        int Len() => Mathf.Max(1, length.Value);
    }
}
