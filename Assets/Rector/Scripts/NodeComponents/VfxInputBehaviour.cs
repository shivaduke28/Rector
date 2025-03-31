using System;
using System.Collections.Generic;
using System.Linq;
using R3;
using Rector.Nodes;
using UnityEngine;
using UnityEngine.VFX;

namespace Rector.NodeComponents
{
    [RequireComponent(typeof(VisualEffect))]
    public sealed class VfxInputBehaviour : InputBehaviour
    {
        [SerializeField] VisualEffect visualEffect;
        [SerializeField] BoolInput activeInput = new("Active", false);
        [SerializeField] string[] events;

        [Serializable]
        sealed class VfxPropertyInput
        {
            [SerializeReference] public IInput input;
            public bool reinit;

            int? id;
            public int Id => id ??= Shader.PropertyToID(input.Name);
        }

        [SerializeField] VfxPropertyInput[] propertyInputs;

        IInput[] inputs;

        void Reset() => visualEffect = GetComponent<VisualEffect>();

        public override IInput[] GetInputs()
            => inputs ??= events.Select(x => new CallbackInput(x, () => visualEffect.SendEvent(x)))
                .Concat(propertyInputs.Select(x => x.input))
                .Prepend(activeInput)
                .ToArray();


        public void ToggleActive()
        {
            activeInput.Value.Value = !activeInput.Value.Value;
        }

        void Start()
        {
            activeInput.Value.Subscribe(x => visualEffect.enabled = x);
            foreach (var propertyInput in propertyInputs)
            {
                switch (propertyInput.input)
                {
                    case FloatInput floatInput:
                        floatInput.Value.Subscribe(x =>
                        {
                            visualEffect.SetFloat(propertyInput.Id, x);
                            if (propertyInput.reinit)
                            {
                                visualEffect.Reinit();
                            }
                        }).AddTo(this);
                        break;
                    case IntInput intInput:
                        intInput.Value.Subscribe(x =>
                        {
                            visualEffect.SetInt(propertyInput.Id, x);
                            if (propertyInput.reinit)
                            {
                                visualEffect.Reinit();
                            }
                        }).AddTo(this);
                        break;
                    case BoolInput boolInput:
                        boolInput.Value.Subscribe(x =>
                        {
                            visualEffect.SetBool(propertyInput.Id, x);
                            if (propertyInput.reinit)
                            {
                                visualEffect.Reinit();
                            }
                        }).AddTo(this);
                        break;
                    case Vector3Input vector3Input:
                        vector3Input.Value.Subscribe(x =>
                        {
                            visualEffect.SetVector3(propertyInput.Id, x);
                            if (propertyInput.reinit)
                            {
                                visualEffect.Reinit();
                            }
                        }).AddTo(this);
                        break;
                    default:
                        throw new NotImplementedException();
                }
            }
        }


#if UNITY_EDITOR
        public VisualEffect VisualEffect => visualEffect;

        public void ResetProperties(IEnumerable<IInput> infos)
        {
            propertyInputs = infos.Select(x => new VfxPropertyInput
            {
                input = x,
                reinit = false,
            }).ToArray();
        }

        public void ResetEvents(IEnumerable<string> events)
        {
            this.events = events.Where(x => x != VisualEffectAsset.PlayEventName && x != VisualEffectAsset.StopEventName)
                .ToArray();
        }
#endif
    }
}
