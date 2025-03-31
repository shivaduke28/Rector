using R3;
using Rector.NodeBehaviours;
using UnityEngine;
using UnityEngine.VFX;

namespace Rector.NodeComponents
{
    public sealed class PositionInputBehaviour : InputBehaviour
    {
        [SerializeField] VisualEffect visualEffect;
        [SerializeField] Vector3Input input = new("Position", Vector3.zero);
        [SerializeField] bool reinit;

        Transform trans;
        int propertyId = -1;

        void Start()
        {
            trans = transform;

            if (visualEffect != null)
            {
                propertyId = Shader.PropertyToID(input.Name);
            }

            input.Value.Subscribe(p =>
            {
                trans.localPosition = p;
                if (visualEffect != null)
                {
                    visualEffect.SetVector3(propertyId, p);
                    if (reinit)
                    {
                        visualEffect.Reinit();
                    }
                }
            }).AddTo(this);
        }

        public override IInput[] GetInputs()
        {
            return new IInput[]
            {
                input,
            };
        }

        void Reset()
        {
            input = new Vector3Input("Position", transform.localPosition);
        }
    }
}
