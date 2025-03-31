using System;
using R3;
using Rector.NodeBehaviours;
using Unity.Cinemachine;
using UnityEngine;

namespace Rector.Cameras
{
    [RequireComponent(typeof(CinemachineSplineDolly))]
    public class CinemachineDollyInputBehaviour : InputBehaviour
    {
        [SerializeField] CinemachineSplineDolly dolly;
        [SerializeField] FloatInput speed;
        [SerializeField] Vector3Input offset;

        IInput[] inputs;

        public override IInput[] GetInputs()
        {
            return inputs ??= new IInput[] { speed, offset };
        }

        void Start()
        {
            speed.Value.Subscribe(UpdateSpeed).AddTo(this);
            offset.Value.Subscribe(x => dolly.SplineOffset = x).AddTo(this);
        }

        void UpdateSpeed(float x)
        {
            var automaticDolly = dolly.AutomaticDolly;
            var method = automaticDolly.Method;
            if (method is SplineAutoDolly.FixedSpeed fixedSpeed)
            {
                fixedSpeed.Speed = x;
                automaticDolly.Method = fixedSpeed;
                dolly.AutomaticDolly = automaticDolly;
            }
        }

        void Reset()
        {
            dolly = GetComponent<CinemachineSplineDolly>();

            var defaultSpeed = 0.1f;
            if (TryGetFixedSpeed(out var fixedSpeed))
            {
                defaultSpeed = fixedSpeed.Speed;
            }

            speed = new FloatInput("Speed", defaultSpeed, 0f, defaultSpeed * 4f);
            offset = new Vector3Input("Offset", dolly.SplineOffset);
        }

        bool TryGetFixedSpeed(out SplineAutoDolly.FixedSpeed fixedSpeed)
        {
            var method = dolly.AutomaticDolly.Method;
            if (method is SplineAutoDolly.FixedSpeed fs)
            {
                fixedSpeed = fs;
                return true;
            }

            fixedSpeed = null;
            return false;
        }
    }
}
