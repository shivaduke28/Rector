using System;
using System.Collections.Generic;
using System.Linq;
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

        public IInput[] GetInputs() => proxyInputs.OfType<IInput>().ToArray();

        public IOutput[] GetOutputs() => proxyOutputs.OfType<IOutput>().ToArray();

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
    }
}
