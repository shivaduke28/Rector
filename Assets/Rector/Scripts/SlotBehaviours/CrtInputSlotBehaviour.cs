using Rector.NodeBehaviours;
using UnityEngine;

namespace Rector.SlotBehaviours
{
    // NOTE: Texture型をサポートしたらOutputもはやして良い
    [AddComponentMenu("Rector/CRT Input Slot")]
    public sealed class CrtInputSlotBehaviour : InputSlotBehaviour
    {
        [SerializeField] CustomRenderTexture crt;

        IInput[] inputs;

        public override IInput[] GetInputs()
        {
            if (inputs == null)
            {
                inputs = new IInput[]
                {
                    new CallbackInput("Init", crt.Initialize),
                    new CallbackInput("Update", crt.Update),
                };
            }
            return inputs;
        }
    }
}
