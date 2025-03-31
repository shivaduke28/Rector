using System;
using System.Collections.Generic;
using System.Linq;
using R3;
using Rector.NodeBehaviours;
using Rector.SlotBehaviours;
using UnityEngine;
using UnityEngine.VFX;

namespace Rector.Vfx
{
    [AddComponentMenu("Rector/VFX Input Slot")]
    [RequireComponent(typeof(VisualEffect))]
    public sealed class VfxInputSlotBehaviour : InputSlotBehaviour
    {
        [SerializeField] VisualEffect visualEffect;
        [SerializeField] BoolInput activeInput = new("Active", false);
        [SerializeField] string[] events;
        [SerializeReference, SelectableSerializeReference] IInput[] properties;

        IInput[] inputs;

        void Reset() => visualEffect = GetComponent<VisualEffect>();

        public override IInput[] GetInputs()
            => inputs ??= events.Select(x => new CallbackInput(x, () => visualEffect.SendEvent(x)))
                .Concat(properties)
                .Prepend(activeInput)
                .ToArray();


        public void ToggleActive()
        {
            activeInput.Value.Value = !activeInput.Value.Value;
        }

        void Start()
        {
            activeInput.Value.Subscribe(x => visualEffect.enabled = x).AddTo(this);
            foreach (var property in properties)
            {
                var id = Shader.PropertyToID(property.Name);
                switch (property)
                {
                    case FloatInput floatInput:
                        floatInput.Value.Subscribe(id, (x, i) => { visualEffect.SetFloat(i, x); }).AddTo(this);
                        break;
                    case IntInput intInput:
                        intInput.Value.Subscribe(id, (x, i) => { visualEffect.SetInt(i, x); }).AddTo(this);
                        break;
                    case BoolInput boolInput:
                        boolInput.Value.Subscribe(id, (x, i) => { visualEffect.SetBool(i, x); }).AddTo(this);
                        break;
                    case Vector3Input vector3Input:
                        vector3Input.Value.Subscribe(id, (x, i) => { visualEffect.SetVector3(i, x); }).AddTo(this);
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
            properties = infos.ToArray();
        }

        public void ResetEvents(IEnumerable<string> events)
        {
            this.events = events.Where(x => x != VisualEffectAsset.PlayEventName && x != VisualEffectAsset.StopEventName)
                .ToArray();
        }
#endif
    }
}
