using System;
using R3;
using Rector.Audio;
using Rector.UI.Graphs.Slots;

namespace Rector.UI.Graphs.Nodes
{
    public sealed class SequenceNode : Node, IInitializable, IDisposable
    {
        public const string NodeName = "Seq";
        public static NodeCategory GetCategory() => NodeCategory.Sequence;
        public override NodeCategory Category => GetCategory();
        public override InputSlot[] InputSlots { get; }
        public override OutputSlot[] OutputSlots { get; }

        readonly SequenceModel sequenceModel;
        readonly ReactiveProperty<int> length = new(SequenceModel.DefaultLength);
        readonly CompositeDisposable disposable = new(2);

        public ReadOnlyReactiveProperty<int> Beat => sequenceModel.BeatProperty;
        public ReadOnlyReactiveProperty<int> Length => sequenceModel.LengthProperty;

        public SequenceNode(NodeId id, SequenceModel sequenceModel) : base(id, NodeName)
        {
            this.sequenceModel = sequenceModel;
            InputSlots = new InputSlot[]
            {
                new CallbackInputSlot(id, 0, "Step", sequenceModel.Step, IsMuted),
                new CallbackInputSlot(id, 1, "Reset", sequenceModel.Reset, IsMuted),
                new ReactivePropertyIntInputSlot(id, 2, "Length", length, SequenceModel.DefaultLength, SequenceModel.MinLength, SequenceModel.MaxLength, IsMuted)
            };

            OutputSlots = new OutputSlot[]
            {
                new ObservableOutputSlot<int>(id, 0, "Beat", sequenceModel.BeatProperty, IsMuted)
            };
        }

        public void Initialize()
        {
            // モデル→スロットを先に張る: リプレイでスロットが現在のグローバル値に揃い、
            // 直後のスロット→モデルのリプレイがno-opになる（逆順だと既定値がモデルを上書きする）
            sequenceModel.LengthProperty.Subscribe(x => length.Value = x).AddTo(disposable);
            length.Subscribe(sequenceModel.SetLength).AddTo(disposable);
        }

        public override void DoAction() => sequenceModel.Step();

        public void Dispose()
        {
            disposable.Dispose();
        }
    }
}
