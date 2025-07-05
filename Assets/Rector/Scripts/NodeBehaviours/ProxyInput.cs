using System;
using R3;
using UnityEngine;

namespace Rector.NodeBehaviours
{
    // Base classes for proxy inputs
    internal abstract class ProxyInput : IInput
    {
        public abstract string Name { get; }
        public abstract void UpdateInput(IInput input);
    }

    // Proxy implementations for each input type
    internal sealed class ProxyFloatInput : ProxyInput, IFloatInput
    {
        FloatInput currentInput;
        readonly ReactiveProperty<float> value;
        readonly float minValue;
        readonly float maxValue;
        readonly float defaultValue;
        IDisposable subscription;

        public ProxyFloatInput(FloatInput input)
        {
            currentInput = input;
            value = new ReactiveProperty<float>(input.Value.Value);
            minValue = input.MinValue;
            maxValue = input.MaxValue;
            defaultValue = input.DefaultValue;
            Name = input.Name;

            subscription =
                new CompositeDisposable(value.Subscribe(v => currentInput.Value.Value = v),
                    currentInput.Value.Subscribe(v => value.Value = v)
                );
        }

        public override string Name { get; }
        public ReactiveProperty<float> Value => value;
        public float DefaultValue => defaultValue;
        public float MinValue => minValue;
        public float MaxValue => maxValue;


        public override void UpdateInput(IInput input)
        {
            if (input is FloatInput floatInput)
            {
                currentInput = floatInput;
                subscription?.Dispose();
                subscription =
                    new CompositeDisposable(value.Subscribe(v => currentInput.Value.Value = v),
                        currentInput.Value.Subscribe(v => value.Value = v)
                    );
            }
        }
    }

    internal sealed class ProxyIntInput : ProxyInput, IIntInput
    {
        IntInput currentInput;
        readonly ReactiveProperty<int> value;
        readonly int minValue;
        readonly int maxValue;
        readonly int defaultValue;
        IDisposable subscription;

        public ProxyIntInput(IntInput input)
        {
            currentInput = input;
            value = new ReactiveProperty<int>(input.Value.Value);
            minValue = input.MinValue;
            maxValue = input.MaxValue;
            defaultValue = input.DefaultValue;
            Name = input.Name;

            subscription =
                new CompositeDisposable(value.Subscribe(v => currentInput.Value.Value = v),
                    currentInput.Value.Subscribe(v => value.Value = v)
                );
        }

        public override string Name { get; }
        public ReactiveProperty<int> Value => value;
        public int DefaultValue => defaultValue;
        public int MinValue => minValue;
        public int MaxValue => maxValue;


        public override void UpdateInput(IInput input)
        {
            if (input is IntInput intInput)
            {
                currentInput = intInput;
                subscription?.Dispose();
                subscription =
                    new CompositeDisposable(value.Subscribe(v => currentInput.Value.Value = v),
                        currentInput.Value.Subscribe(v => value.Value = v)
                    );
            }
        }
    }

    internal sealed class ProxyVector3Input : ProxyInput, IVector3Input
    {
        Vector3Input currentInput;
        readonly ReactiveProperty<Vector3> value;
        readonly Vector3 defaultValue;
        IDisposable subscription;

        public ProxyVector3Input(Vector3Input input)
        {
            currentInput = input;
            value = new ReactiveProperty<Vector3>(input.Value.Value);
            defaultValue = input.DefaultValue;
            Name = input.Name;

            subscription =
                new CompositeDisposable(value.Subscribe(v => currentInput.Value.Value = v),
                    currentInput.Value.Subscribe(v => value.Value = v)
                );
        }

        public override string Name { get; }
        public ReactiveProperty<Vector3> Value => value;
        public Vector3 DefaultValue => defaultValue;


        public override void UpdateInput(IInput input)
        {
            if (input is Vector3Input vector3Input)
            {
                currentInput = vector3Input;
                subscription?.Dispose();
                subscription =
                    new CompositeDisposable(value.Subscribe(v => currentInput.Value.Value = v),
                        currentInput.Value.Subscribe(v => value.Value = v)
                    );
            }
        }
    }

    internal sealed class ProxyBoolInput : ProxyInput, IBoolInput
    {
        BoolInput currentInput;
        readonly ReactiveProperty<bool> value;
        readonly bool defaultValue;
        IDisposable subscription;

        public ProxyBoolInput(BoolInput input)
        {
            currentInput = input;
            value = new ReactiveProperty<bool>(input.Value.Value);
            defaultValue = input.DefaultValue;
            Name = input.Name;

            subscription =
                new CompositeDisposable(value.Subscribe(v => currentInput.Value.Value = v),
                    currentInput.Value.Subscribe(v => value.Value = v)
                );
        }

        public override string Name { get; }
        public ReactiveProperty<bool> Value => value;
        public bool DefaultValue => defaultValue;


        public override void UpdateInput(IInput input)
        {
            if (input is BoolInput boolInput)
            {
                currentInput = boolInput;
                subscription?.Dispose();
                subscription =
                    new CompositeDisposable(value.Subscribe(v => currentInput.Value.Value = v),
                        currentInput.Value.Subscribe(v => value.Value = v)
                    );
            }
        }
    }

    internal sealed class ProxyTransformInput : ProxyInput, ITransformInput
    {
        TransformInput currentInput;
        readonly ReactiveProperty<Transform> value;
        readonly Transform defaultValue;
        IDisposable subscription;

        public ProxyTransformInput(TransformInput input)
        {
            currentInput = input;
            value = new ReactiveProperty<Transform>(input.Value.Value);
            defaultValue = input.DefaultValue;
            Name = input.Name;

            subscription =
                new CompositeDisposable(value.Subscribe(v => currentInput.Value.Value = v),
                    currentInput.Value.Subscribe(v => value.Value = v)
                );
        }

        public override string Name { get; }
        public ReactiveProperty<Transform> Value => value;
        public Transform DefaultValue => defaultValue;


        public override void UpdateInput(IInput input)
        {
            if (input is TransformInput transformInput)
            {
                currentInput = transformInput;
                subscription?.Dispose();
                subscription =
                    new CompositeDisposable(value.Subscribe(v => currentInput.Value.Value = v),
                        currentInput.Value.Subscribe(v => value.Value = v)
                    );
            }
        }
    }

    internal sealed class ProxyCallbackInput : ProxyInput, ICallbackInput
    {
        readonly Subject<Unit> subject = new();
        CallbackInput currentInput;
        IDisposable subscription;

        public ProxyCallbackInput(CallbackInput input)
        {
            currentInput = input;
            Name = input.Name;
            Callback = () => subject.OnNext(Unit.Default);
            subscription = subject.Subscribe(_ => currentInput.Callback?.Invoke());
        }

        public override string Name { get; }
        public Action Callback { get; }


        public override void UpdateInput(IInput input)
        {
            if (input is CallbackInput callbackInput)
            {
                currentInput = callbackInput;
                subscription?.Dispose();
                subscription = subject.Subscribe(_ => currentInput.Callback?.Invoke());
            }
        }
    }
}