using R3;
using Rector.UI.Graphs;
using UnityEngine;

namespace Rector.UI.Nodes
{
    public sealed class CrtNode : Node
    {
        readonly CustomRenderTexture crt;

        public CrtNode(NodeId id, CustomRenderTexture crt) : base(id, crt.name)
        {
            this.crt = crt;
            InputSlots = new InputSlot[]
            {
                new CallbackInputSlot(id, 0, "Init", crt.Initialize, IsMuted),
                new CallbackInputSlot(id, 1, "Update", crt.Update, IsMuted),
            };
            OutputSlots = new OutputSlot[]
            {
                new ObservableOutputSlot<Texture>(id, 0, "Texture", Observable.Return<Texture>(crt), IsMuted)
            };
        }

        public override InputSlot[] InputSlots { get; }
        public override OutputSlot[] OutputSlots { get; }
    }
}
