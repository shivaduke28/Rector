using System;
using R3;
using Rector.Osc;
using Rector.UI.Graphs.Slots;

namespace Rector.UI.Graphs.Nodes
{
    /// <summary>
    /// OSC アドレスを1つ受けるノード。アドレスはラーンでのみ確定する。
    /// </summary>
    /// <remarks>
    /// アドレスは入力スロットにしていない。NodeBehaviours に文字列の入力型が無く、
    /// 画面上で打ち込む手段も無いので、ノード内部の状態として持つ。
    /// </remarks>
    public sealed class OscNode : LearnableSourceNode
    {
        public const string NodeName = "OSC";
        public override InputSlot[] InputSlots { get; }
        public override OutputSlot[] OutputSlots { get; }

        readonly ReactiveProperty<string> address = new("");
        readonly OscModel oscModel;

        public OscNode(NodeId id, OscModel oscModel) : base(id, NodeName)
        {
            this.oscModel = oscModel;

            var matched = oscModel.Messages.Where(m => m.Address == address.Value);
            var valued = matched.Where(m => m.HasValue);

            DisplayLabel = address.Select(a => string.IsNullOrEmpty(a) ? NodeName : $"{NodeName} {a}");
            DisplayValue = valued.Select(m => m.Value);

            InputSlots = new[]
            {
                SlotConverter.Convert(id, 0, ActiveInput, IsMuted),
                SlotConverter.Convert(id, 1, LearnInput, IsMuted)
            };

            OutputSlots = new OutputSlot[]
            {
                new ObservableOutputSlot<float>(id, 0, "Value",
                    valued.Where(_ => IsActive).Select(m => m.Value), IsMuted),

                // 引数の有無でも値でも絞らない。float 出力を Unit 入力へ繋ぐ経路は
                // EdgeConnector が 0 を落とすので、引数ゼロの bang もボタンの解放も
                // Value 経由では拾えない。それを拾うための口
                new ObservableOutputSlot<Unit>(id, 1, "Event",
                    matched.Where(_ => IsActive).AsUnitObservable(), IsMuted)
            };
        }

        protected override IDisposable SubscribeLearn()
            => oscModel.Messages
                .Take(1)
                .Subscribe(m =>
                {
                    address.Value = m.Address;
                    Disarm(m.Address);
                });
    }
}
