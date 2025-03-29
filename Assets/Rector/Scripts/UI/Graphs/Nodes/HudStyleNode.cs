using System;
using R3;
using Rector.Nodes;
using Rector.UI.Graphs.Slots;
using Rector.UI.Hud;
using UnityEngine;

namespace Rector.UI.Graphs.Nodes
{
    public class HudStyleNode : Node, IInitializable, IDisposable
    {
        public const string NodeName = "HUD Style";
        public static string Category => NodeCategoryV2.System;
        readonly HudModel hudModel;
        readonly CompositeDisposable disposable = new(2);

        readonly FloatInput r = new("R", 0, 0, 1);
        readonly FloatInput g = new("G", 0, 0, 1);
        readonly FloatInput b = new("B", 0, 0, 1);
        readonly FloatInput a = new("A", 0, 0, 1);

        public HudStyleNode(NodeId id, HudModel hudModel) : base(id, NodeName)
        {
            this.hudModel = hudModel;
            InputSlots = new[]
            {
                SlotConverter.Convert(id, 0, r, IsMuted),
                SlotConverter.Convert(id, 1, g, IsMuted),
                SlotConverter.Convert(id, 2, b, IsMuted),
                SlotConverter.Convert(id, 3, a, IsMuted)
            };
        }

        public override InputSlot[] InputSlots { get; }
        public override OutputSlot[] OutputSlots { get; } = Array.Empty<OutputSlot>();

        public void Dispose()
        {
            disposable.Dispose();
        }

        public void Initialize()
        {
            r.Value.CombineLatest(g.Value, b.Value, a.Value, (r1, g1, b1, a1) => new Color(r1, g1, b1, a1))
                .Subscribe(c => hudModel.FrameColor.Value = c)
                .AddTo(disposable);
            hudModel.FrameColor.Subscribe(c =>
            {
                r.Value.Value = c.r;
                g.Value.Value = c.g;
                b.Value.Value = c.b;
                a.Value.Value = c.a;
            }).AddTo(disposable);
        }
    }
}
