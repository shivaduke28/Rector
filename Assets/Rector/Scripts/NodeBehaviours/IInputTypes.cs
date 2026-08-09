using System;
using R3;
using UnityEngine;

namespace Rector.NodeBehaviours
{
    public interface IFloatInput : IInput
    {
        ReactiveProperty<float> Value { get; }
        float DefaultValue { get; }
        float MinValue { get; }
        float MaxValue { get; }
    }

    public interface IIntInput : IInput
    {
        ReactiveProperty<int> Value { get; }
        int DefaultValue { get; }
        int MinValue { get; }
        int MaxValue { get; }
    }

    public interface IVector3Input : IInput
    {
        ReactiveProperty<Vector3> Value { get; }
        Vector3 DefaultValue { get; }
    }

    public interface IBoolInput : IInput
    {
        ReactiveProperty<bool> Value { get; }
        bool DefaultValue { get; }
    }

    public interface ITransformInput : IInput
    {
        ReactiveProperty<Transform> Value { get; }
        Transform DefaultValue { get; }
    }

    public interface ICallbackInput : IInput
    {
        Action Callback { get; }
    }
}
