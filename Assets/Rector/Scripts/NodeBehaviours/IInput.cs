using System;
using R3;
using UnityEngine;

namespace Rector.NodeBehaviours
{
    public interface IInput
    {
        string Name { get; }
    }

    public sealed class CallbackInput : ICallbackInput
    {
        public string Name { get; }
        public Action Callback { get; }

        public CallbackInput(string name, Action callback)
        {
            Name = name;
            Callback = callback;
        }
    }

#nullable enable
    [Serializable]
    public abstract class ValueInput<T> : IInput
    {
        [SerializeField] string name;
        [SerializeField] SerializableReactiveProperty<T> value;
        [SerializeField] T? defaultValue;
        public string Name => name;
        public ReactiveProperty<T> Value => value;

        public T DefaultValue => defaultValue ??= value.Value;

        protected ValueInput(string name, T defaultValue)
        {
            this.name = name;
            this.defaultValue = defaultValue;
            value = new SerializableReactiveProperty<T>(defaultValue);
        }
    }
#nullable disable

    [Serializable]
    public sealed class FloatInput : ValueInput<float>, IFloatInput
    {
        [SerializeField] float minValue;
        [SerializeField] float maxValue;
        public float MinValue => minValue;
        public float MaxValue => maxValue;

        public FloatInput(string name, float defaultValue, float minValue, float maxValue) : base(name, defaultValue)
        {
            this.minValue = minValue;
            this.maxValue = maxValue;
        }

        public FloatInput() : base("", 0)
        {
            minValue = 0f;
            maxValue = 1f;
        }
    }

    [Serializable]
    public sealed class IntInput : ValueInput<int>, IIntInput
    {
        [SerializeField] int minValue;
        [SerializeField] int maxValue;

        public int MinValue => minValue;
        public int MaxValue => maxValue;

        public IntInput(string name, int defaultValue, int minValue, int maxValue) : base(name, defaultValue)
        {
            this.minValue = minValue;
            this.maxValue = maxValue;
        }

        public IntInput() : base("", 0)
        {
            minValue = 0;
            maxValue = 1;
        }
    }

    [Serializable]
    public sealed class Vector3Input : ValueInput<Vector3>, IVector3Input
    {
        public Vector3Input(string name, Vector3 defaultValue) : base(name, defaultValue)
        {
        }

        public Vector3Input() : base("", Vector3.zero)
        {
        }
    }

    [Serializable]
    public sealed class BoolInput : ValueInput<bool>, IBoolInput
    {
        public BoolInput(string name, bool defaultValue) : base(name, defaultValue)
        {
        }

        public BoolInput() : base("", false)
        {
        }
    }


    [Serializable]
    public sealed class TransformInput : ValueInput<Transform>, ITransformInput
    {
        public TransformInput(string name, Transform defaultValue) : base(name, defaultValue)
        {
        }

        public TransformInput() : base("", null)
        {
        }
    }
}
