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
        IDisposable subscription;

        public ProxyFloatInput(FloatInput input)
        {
            currentInput = input;
            Value = new ReactiveProperty<float>(input.Value.Value);
            MinValue = input.MinValue;
            MaxValue = input.MaxValue;
            DefaultValue = input.DefaultValue;
            Name = input.Name;

            subscription =
                new CompositeDisposable(Value.Subscribe(v => currentInput.Value.Value = v),
                    currentInput.Value.Subscribe(v => Value.Value = v)
                );
        }

        public override string Name { get; }
        public ReactiveProperty<float> Value { get; }
        public float DefaultValue { get; }
        public float MinValue { get; }
        public float MaxValue { get; }


        public override void UpdateInput(IInput input)
        {
            if (input is FloatInput floatInput)
            {
                currentInput = floatInput;
                subscription?.Dispose();
                subscription =
                    new CompositeDisposable(Value.Subscribe(v => currentInput.Value.Value = v),
                            currentInput.Value.Subscribe(v => Value.Value = v)
                    );
            }
        }
    }

    internal sealed class ProxyIntInput : ProxyInput, IIntInput
    {
        IntInput currentInput;
        IDisposable subscription;

        public ProxyIntInput(IntInput input)
        {
            currentInput = input;
            Value = new ReactiveProperty<int>(input.Value.Value);
            MinValue = input.MinValue;
            MaxValue = input.MaxValue;
            DefaultValue = input.DefaultValue;
            Name = input.Name;

            subscription =
                new CompositeDisposable(Value.Subscribe(v => currentInput.Value.Value = v),
                    currentInput.Value.Subscribe(v => Value.Value = v)
                );
        }

        public override string Name { get; }
        public ReactiveProperty<int> Value { get; }
        public int DefaultValue { get; }
        public int MinValue { get; }
        public int MaxValue { get; }


        public override void UpdateInput(IInput input)
        {
            if (input is IntInput intInput)
            {
                currentInput = intInput;
                subscription?.Dispose();
                subscription =
                    new CompositeDisposable(Value.Subscribe(v => currentInput.Value.Value = v),
                            currentInput.Value.Subscribe(v => Value.Value = v)
                    );
            }
        }
    }

    internal sealed class ProxyVector3Input : ProxyInput, IVector3Input
    {
        Vector3Input currentInput;
        IDisposable subscription;

        public ProxyVector3Input(Vector3Input input)
        {
            currentInput = input;
            Value = new ReactiveProperty<Vector3>(input.Value.Value);
            DefaultValue = input.DefaultValue;
            Name = input.Name;

            subscription =
                new CompositeDisposable(Value.Subscribe(v => currentInput.Value.Value = v),
                    currentInput.Value.Subscribe(v => Value.Value = v)
                );
        }

        public override string Name { get; }
        public ReactiveProperty<Vector3> Value { get; }
        public Vector3 DefaultValue { get; }


        public override void UpdateInput(IInput input)
        {
            if (input is Vector3Input vector3Input)
            {
                currentInput = vector3Input;
                subscription?.Dispose();
                subscription =
                    new CompositeDisposable(Value.Subscribe(v => currentInput.Value.Value = v),
                            currentInput.Value.Subscribe(v => Value.Value = v)
                    );
            }
        }
    }

    internal sealed class ProxyBoolInput : ProxyInput, IBoolInput
    {
        BoolInput currentInput;
        IDisposable subscription;

        public ProxyBoolInput(BoolInput input)
        {
            currentInput = input;
            Value = new ReactiveProperty<bool>(input.Value.Value);
            DefaultValue = input.DefaultValue;
            Name = input.Name;

            subscription =
                new CompositeDisposable(Value.Subscribe(v => currentInput.Value.Value = v),
                    currentInput.Value.Subscribe(v => Value.Value = v)
                );
        }

        public override string Name { get; }
        public ReactiveProperty<bool> Value { get; }
        public bool DefaultValue { get; }


        public override void UpdateInput(IInput input)
        {
            if (input is BoolInput boolInput)
            {
                currentInput = boolInput;
                subscription?.Dispose();
                subscription =
                    new CompositeDisposable(Value.Subscribe(v => currentInput.Value.Value = v),
                            currentInput.Value.Subscribe(v => Value.Value = v)
                    );
            }
        }
    }

    internal sealed class ProxyTransformInput : ProxyInput, ITransformInput
    {
        TransformInput currentInput;
        IDisposable subscription;

        public ProxyTransformInput(TransformInput input)
        {
            currentInput = input;
            Value = new ReactiveProperty<Transform>(input.Value.Value);
            DefaultValue = input.DefaultValue;
            Name = input.Name;

            subscription =
                new CompositeDisposable(Value.Subscribe(v => currentInput.Value.Value = v),
                    currentInput.Value.Subscribe(v => Value.Value = v)
                );
        }

        public override string Name { get; }
        public ReactiveProperty<Transform> Value { get; }
        public Transform DefaultValue { get; }


        public override void UpdateInput(IInput input)
        {
            if (input is TransformInput transformInput)
            {
                currentInput = transformInput;
                subscription?.Dispose();
                subscription =
                    new CompositeDisposable(Value.Subscribe(v => currentInput.Value.Value = v),
                            currentInput.Value.Subscribe(v => Value.Value = v)
                    );
            }
        }
    }

    internal sealed class ProxyCallbackInput : ProxyInput, ICallbackInput
    {
        CallbackInput currentInput;
        readonly Subject<Unit> subject = new();
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