using R3;
using Rector.NodeBehaviours;
using UnityEngine;

namespace Rector.NodeComponents
{
    public class ScaleInputBehaviour : InputBehaviour
    {
        [SerializeField] FloatInput size = new("Size", 1f, 0f, 10f);
        [SerializeField] Vector3Input scale = new("Scale", Vector3.one);

        const float MinScale = 0.01f;

        IInput[] inputs;

        void Start()
        {
            size.Value.CombineLatest(scale.Value, (si, sc) => sc * si)
                .Select(s => s = new Vector3(
                    Mathf.Max(MinScale, Mathf.Abs(s.x)),
                    Mathf.Max(MinScale, Mathf.Abs(s.y)),
                    Mathf.Max(MinScale, Mathf.Abs(s.z))))
                .Subscribe(transform, (s, t) => t.localScale = s).AddTo(this);
        }

        public override IInput[] GetInputs()
        {
            return inputs ??= new IInput[] { size, scale, };
        }

        void Reset()
        {
            size.Value.Value = 1f;
            scale.Value.Value = transform.localScale;
        }
    }
}
