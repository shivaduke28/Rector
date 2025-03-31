using Rector.NodeBehaviours;
using UnityEngine;

namespace Rector.NodeComponents
{
    public sealed class CrtInputBehaviour : InputBehaviour
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
