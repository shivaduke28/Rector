using System;
using R3;
using Rector.NodeBehaviours;
using Rector.UI.Graphs.Slots;
using Rector.UI.Hud;
using UnityEngine;

namespace Rector.UI.Graphs.Nodes
{
    public class HudStyleNode : Node, IInitializable, IDisposable
    {
        public const string NodeName = "HUD Style";
        public static NodeCategory GetCategory() => NodeCategory.System;
        public override NodeCategory Category => GetCategory();
        readonly HudModel hudModel;
        readonly CompositeDisposable disposable = new(2);

        readonly FloatInput h = new("H", 0, 0, 1);
        readonly FloatInput s = new("S", 0, 0, 1);
        readonly FloatInput v = new("V", 0, 0, 1);
        readonly FloatInput a = new("A", 0, 0, 1);

        public HudStyleNode(NodeId id, HudModel hudModel) : base(id, NodeName)
        {
            this.hudModel = hudModel;
            InputSlots = new[]
            {
                SlotConverter.Convert(id, 0, h, IsMuted),
                SlotConverter.Convert(id, 1, s, IsMuted),
                SlotConverter.Convert(id, 2, v, IsMuted),
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
            h.Value.CombineLatest(s.Value, v.Value, a.Value, (h1, s1, v1, a1) => 
                {
                    var color = Color.HSVToRGB(h1, s1, v1);
                    color.a = a1;
                    return color;
                })
                .Subscribe(c => hudModel.FrameColor.Value = c)
                .AddTo(disposable);
            hudModel.FrameColor.Subscribe(c =>
            {
                Color.RGBToHSV(c, out var hue, out var saturation, out var value);
                // V=0の時はH,Sの値を更新しない（黒色で彩度情報が失われるため）
                if (value > 0)
                {
                    h.Value.Value = hue;
                    s.Value.Value = saturation;
                }
                v.Value.Value = value;
                a.Value.Value = c.a;
            }).AddTo(disposable);
        }
    }
}
