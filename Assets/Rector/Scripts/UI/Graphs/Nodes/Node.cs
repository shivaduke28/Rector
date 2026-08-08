using R3;
using Rector.UI.Graphs.Slots;

namespace Rector.UI.Graphs.Nodes
{
    public abstract class Node
    {
        public NodeId Id { get; }
        public string Name { get; }

        /// <summary>このノードを生んだ NodeTemplate の Id。</summary>
        public NodeTemplateId TemplateId { get; internal set; }

        /// <summary>グラフの保存に含めてよいか。テンプレートから引き継ぐ。</summary>
        public bool IsSaveable { get; internal set; }

        public abstract NodeCategory Category { get; }
        public abstract InputSlot[] InputSlots { get; }
        public abstract OutputSlot[] OutputSlots { get; }
        public readonly ReactiveProperty<bool> Selected = new(false);

        /// <summary>エッジ作成のターゲットとして指されている。ソース(Selected)とは別の見た目になる。</summary>
        public readonly ReactiveProperty<bool> IsTarget = new(false);

        /// <summary>GrabModifier(R2)で掴まれている。ノードの左右に点滅する矢印が出る。</summary>
        public readonly ReactiveProperty<bool> IsGrabbed = new(false);

        public readonly ReactiveProperty<bool> IsMuted = new(false);

        public virtual void DoAction()
        {
        }

        protected Node(NodeId id, string name)
        {
            Id = id;
            Name = name;
        }
    }
}
