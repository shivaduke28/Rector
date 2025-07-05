using System;
using System.Collections.Generic;
using R3;
using Rector.UI.Graphs;
using UnityEngine;

namespace Rector.NodeBehaviours
{
    public sealed class NodeBehaviourProxy
    {
        readonly ProxyInput[] proxyInputs;
        readonly ProxyOutput[] proxyOutputs;

        NodeBehaviour nodeBehaviour;

        public Guid Guid { get; }

        public string Name { get; }

        public string Category { get; }

        public bool IsDestroyed => nodeBehaviour == null;

        public NodeBehaviourProxy(Guid guid, NodeBehaviour nodeBehaviour)
        {
            this.Guid = guid;
            this.nodeBehaviour = nodeBehaviour;
            Name = nodeBehaviour.Name;
            Category = nodeBehaviour.Category;

            // Create proxy inputs
            var inputs = nodeBehaviour.GetInputs();
            var validInputs = new List<ProxyInput>();
            for (var i = 0; i < inputs.Length; i++)
            {
                if (inputs[i] != null)
                {
                    validInputs.Add(CreateProxyInput(inputs[i]));
                }
                else
                {
                    Debug.LogWarning($"NodeBehaviour '{nodeBehaviour.Name}' has null input at index {i}");
                }
            }
            proxyInputs = validInputs.ToArray();

            // Create proxy outputs
            var outputs = nodeBehaviour.GetOutputs();
            var validOutputs = new List<ProxyOutput>();
            for (var i = 0; i < outputs.Length; i++)
            {
                if (outputs[i] != null)
                {
                    validOutputs.Add(CreateProxyOutput(outputs[i]));
                }
                else
                {
                    Debug.LogWarning($"NodeBehaviour '{nodeBehaviour.Name}' has null output at index {i}");
                }
            }
            proxyOutputs = validOutputs.ToArray();
        }

        public void UpdateNodeBehaviour(NodeBehaviour newNodeBehaviour)
        {
            if (newNodeBehaviour == null || newNodeBehaviour.Guid != Guid)
            {
                return;
            }

            nodeBehaviour = newNodeBehaviour;

            // Update proxy inputs
            var inputs = nodeBehaviour.GetInputs();
            for (var i = 0; i < Math.Min(inputs.Length, proxyInputs.Length); i++)
            {
                proxyInputs[i].UpdateInput(inputs[i]);
            }

            // Update proxy outputs
            var outputs = nodeBehaviour.GetOutputs();
            for (var i = 0; i < Math.Min(outputs.Length, proxyOutputs.Length); i++)
            {
                proxyOutputs[i].UpdateOutput(outputs[i]);
            }
        }

        public IInput[] GetInputs()
        {
            var inputs = new IInput[proxyInputs.Length];
            for (var i = 0; i < proxyInputs.Length; i++)
            {
                inputs[i] = proxyInputs[i].GetInput();
            }

            return inputs;
        }

        public IOutput[] GetOutputs()
        {
            var outputs = new IOutput[proxyOutputs.Length];
            for (var i = 0; i < proxyOutputs.Length; i++)
            {
                outputs[i] = proxyOutputs[i].GetOutput();
            }

            return outputs;
        }

        static ProxyInput CreateProxyInput(IInput input)
        {
            if (input == null)
            {
                throw new ArgumentNullException(nameof(input));
            }
            
            return input switch
            {
                FloatInput floatInput => new ProxyFloatInput(floatInput),
                IntInput intInput => new ProxyIntInput(intInput),
                Vector3Input vector3Input => new ProxyVector3Input(vector3Input),
                BoolInput boolInput => new ProxyBoolInput(boolInput),
                TransformInput transformInput => new ProxyTransformInput(transformInput),
                CallbackInput callbackInput => new ProxyCallbackInput(callbackInput),
                _ => throw new ArgumentException($"Unknown input type: {input.GetType()}")
            };
        }

        static ProxyOutput CreateProxyOutput(IOutput output)
        {
            var outputType = output.GetType();
            if (outputType.IsGenericType && outputType.GetGenericTypeDefinition() == typeof(ObservableOutput<>))
            {
                var genericArg = outputType.GetGenericArguments()[0];
                var proxyType = typeof(ProxyObservableOutput<>).MakeGenericType(genericArg);
                return (ProxyOutput)Activator.CreateInstance(proxyType, output);
            }

            throw new ArgumentException($"Unknown output type: {outputType}");
        }

        // Base classes for proxy inputs/outputs
        abstract class ProxyInput : IInput
        {
            public abstract string Name { get; }
            public abstract IInput GetInput();
            public abstract void UpdateInput(IInput input);
        }

        abstract class ProxyOutput : IOutput
        {
            public abstract string Name { get; }
            public abstract IOutput GetOutput();
            public abstract void UpdateOutput(IOutput output);
        }

        // Proxy implementations for each input type
        sealed class ProxyFloatInput : ProxyInput
        {
            FloatInput currentInput;
            readonly FloatInput proxyInput;
            IDisposable subscription;

            public ProxyFloatInput(FloatInput input)
            {
                currentInput = input;
                proxyInput = new FloatInput(input.Name, input.Value.Value, input.MinValue, input.MaxValue);
                subscription =
                    new CompositeDisposable(proxyInput.Value.Subscribe(v => currentInput.Value.Value = v),
                        currentInput.Value.Subscribe(v => proxyInput.Value.Value = v)
                    );
            }

            public override string Name => proxyInput.Name;
            public override IInput GetInput() => proxyInput;

            public override void UpdateInput(IInput input)
            {
                if (input is FloatInput floatInput)
                {
                    currentInput = floatInput;
                    subscription?.Dispose();
                    subscription =
                        new CompositeDisposable(proxyInput.Value.Subscribe(v => currentInput.Value.Value = v),
                            currentInput.Value.Subscribe(v => proxyInput.Value.Value = v)
                        );
                }
            }
        }

        sealed class ProxyIntInput : ProxyInput
        {
            IntInput currentInput;
            readonly IntInput proxyInput;
            IDisposable subscription;

            public ProxyIntInput(IntInput input)
            {
                currentInput = input;
                proxyInput = new IntInput(input.Name, input.Value.Value, input.MinValue, input.MaxValue);
                subscription =
                    new CompositeDisposable(proxyInput.Value.Subscribe(v => currentInput.Value.Value = v),
                        currentInput.Value.Subscribe(v => proxyInput.Value.Value = v)
                    );
            }

            public override string Name => proxyInput.Name;
            public override IInput GetInput() => proxyInput;

            public override void UpdateInput(IInput input)
            {
                if (input is IntInput intInput)
                {
                    currentInput = intInput;
                    subscription?.Dispose();
                    subscription =
                        new CompositeDisposable(proxyInput.Value.Subscribe(v => currentInput.Value.Value = v),
                            currentInput.Value.Subscribe(v => proxyInput.Value.Value = v)
                        );
                }
            }
        }

        sealed class ProxyVector3Input : ProxyInput
        {
            Vector3Input currentInput;
            readonly Vector3Input proxyInput;
            IDisposable subscription;

            public ProxyVector3Input(Vector3Input input)
            {
                currentInput = input;
                proxyInput = new Vector3Input(input.Name, input.Value.Value);
                subscription =
                    new CompositeDisposable(proxyInput.Value.Subscribe(v => currentInput.Value.Value = v),
                        currentInput.Value.Subscribe(v => proxyInput.Value.Value = v)
                    );
            }

            public override string Name => proxyInput.Name;
            public override IInput GetInput() => proxyInput;

            public override void UpdateInput(IInput input)
            {
                if (input is Vector3Input vector3Input)
                {
                    currentInput = vector3Input;
                    subscription?.Dispose();
                    subscription =
                        new CompositeDisposable(proxyInput.Value.Subscribe(v => currentInput.Value.Value = v),
                            currentInput.Value.Subscribe(v => proxyInput.Value.Value = v)
                        );
                }
            }
        }

        sealed class ProxyBoolInput : ProxyInput
        {
            BoolInput currentInput;
            readonly BoolInput proxyInput;
            IDisposable subscription;

            public ProxyBoolInput(BoolInput input)
            {
                currentInput = input;
                proxyInput = new BoolInput(input.Name, input.Value.Value);
                subscription =
                    new CompositeDisposable(proxyInput.Value.Subscribe(v => currentInput.Value.Value = v),
                        currentInput.Value.Subscribe(v => proxyInput.Value.Value = v)
                    );
            }

            public override string Name => proxyInput.Name;
            public override IInput GetInput() => proxyInput;

            public override void UpdateInput(IInput input)
            {
                if (input is BoolInput boolInput)
                {
                    currentInput = boolInput;
                    subscription?.Dispose();
                    subscription =
                        new CompositeDisposable(proxyInput.Value.Subscribe(v => currentInput.Value.Value = v),
                            currentInput.Value.Subscribe(v => proxyInput.Value.Value = v)
                        );
                }
            }
        }

        sealed class ProxyTransformInput : ProxyInput
        {
            TransformInput currentInput;
            readonly TransformInput proxyInput;
            IDisposable subscription;

            public ProxyTransformInput(TransformInput input)
            {
                currentInput = input;
                proxyInput = new TransformInput(input.Name, input.Value.Value);
                subscription =
                    new CompositeDisposable(proxyInput.Value.Subscribe(v => currentInput.Value.Value = v),
                        currentInput.Value.Subscribe(v => proxyInput.Value.Value = v)
                    );
            }

            public override string Name => proxyInput.Name;
            public override IInput GetInput() => proxyInput;

            public override void UpdateInput(IInput input)
            {
                if (input is TransformInput transformInput)
                {
                    currentInput = transformInput;
                    subscription?.Dispose();
                    subscription =
                        new CompositeDisposable(proxyInput.Value.Subscribe(v => currentInput.Value.Value = v),
                            currentInput.Value.Subscribe(v => proxyInput.Value.Value = v)
                        );
                }
            }
        }

        sealed class ProxyCallbackInput : ProxyInput
        {
            readonly CallbackInput proxyInput;
            readonly Subject<Unit> subject = new();
            IDisposable subscription;

            public ProxyCallbackInput(CallbackInput input)
            {
                proxyInput = new CallbackInput(input.Name, () => subject.OnNext(Unit.Default));
                subscription = subject.Subscribe(_ => input.Callback?.Invoke());
            }

            public override string Name => proxyInput.Name;
            public override IInput GetInput() => proxyInput;

            public override void UpdateInput(IInput input)
            {
                if (input is CallbackInput callbackInput)
                {
                    subscription?.Dispose();
                    subscription = subject.Subscribe(_ => callbackInput.Callback?.Invoke());
                }
            }
        }

        sealed class ProxyObservableOutput<T> : ProxyOutput
        {
            ObservableOutput<T> currentOutput;
            readonly ObservableOutput<T> proxyOutput;
            readonly Subject<T> subject = new();
            IDisposable subscription;

            public ProxyObservableOutput(ObservableOutput<T> output)
            {
                currentOutput = output;
                proxyOutput = new ObservableOutput<T>(output.Name, subject);
                subscription = output.Observable.Subscribe(x => subject.OnNext(x));
            }

            public override string Name => proxyOutput.Name;
            public override IOutput GetOutput() => proxyOutput;

            public override void UpdateOutput(IOutput output)
            {
                if (output is ObservableOutput<T> observableOutput)
                {
                    currentOutput = observableOutput;
                    subscription?.Dispose();
                    subscription = observableOutput.Observable.Subscribe(x => subject.OnNext(x));
                }
            }
        }
    }
}
