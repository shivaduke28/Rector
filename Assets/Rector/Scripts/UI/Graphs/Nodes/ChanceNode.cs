using R3;
using Rector.UI.Graphs.Slots;
using UnityEngine;

namespace Rector.UI.Graphs.Nodes
{
    /// <summary>In が来るたびに Chance の確率で Out へ通すゲート。</summary>
    public sealed class ChanceNode : Node
    {
        public const string NodeName = "Chance";
        public static NodeCategory GetCategory() => NodeCategory.Operator;
        public override NodeCategory Category => GetCategory();
        readonly Subject<float> subject = new();
        readonly ReactiveProperty<float> chance = new(0.5f);

        // 直近の抽選結果。レベル出力なので接続時にリプレイされる。
        // 同じ結果が続いても毎回流す（下流の And/Switch に抽選ごとに届かせる）ため、同値を捨てない BehaviorSubject で持つ
        readonly BehaviorSubject<bool> hit = new(false);

        public ReadOnlyReactiveProperty<float> Chance => chance;
        public Observable<bool> Hit => hit;

        public ChanceNode(NodeId id) : base(id, NodeName)
        {
            InputSlots = new InputSlot[]
            {
                new CallbackFloatInputSlot(id, 0, "In", OnIn, float.NegativeInfinity, float.PositiveInfinity, IsMuted),
                new ReactivePropertyFloatInputSlot(id, 1, "Chance", chance, chance.Value, 0f, 1f, IsMuted),
            };

            // Miss は Hit の反転。Negate を挟まずに外れ側の流れを組めるようにする（抽選前は Hit=false / Miss=true）
            OutputSlots = new OutputSlot[]
            {
                new ObservableOutputSlot<float>(id, 0, "Out", subject, IsMuted),
                new ObservableOutputSlot<bool>(id, 1, "Hit", hit, IsMuted),
                new ObservableOutputSlot<bool>(id, 2, "Miss", hit.Select(x => !x), IsMuted)
            };
        }

        // 判定は値が届いた時点で毎回引く。Hit を先に更新してから Out を流すので、
        // Out を受けた側が Hit を見ると今回の結果になっている
        void OnIn(float x)
        {
            // 0 は出来事ではなく消灯信号（Route の Send 0 など）なので、抽選せずに素通しする。
            // 抽選すると外れたときに消灯が捨てられて VFX が点きっぱなしになり、Hit も脱選択のたびに揺れる
            if (x == 0f)
            {
                subject.OnNext(x);
                return;
            }

            var passed = Roll();
            hit.OnNext(passed);
            if (passed) subject.OnNext(x);
        }

        // Random.value は 1.0 を含む閉区間なので、1 のときだけ抽選を省いて必ず通す。chance=0 は決して通らない
        bool Roll() => chance.Value >= 1f || Random.value < chance.Value;

        public override InputSlot[] InputSlots { get; }
        public override OutputSlot[] OutputSlots { get; }
    }
}
