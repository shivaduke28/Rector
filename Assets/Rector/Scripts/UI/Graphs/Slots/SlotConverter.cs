using R3;
using Rector.NodeBehaviours;
using UnityEngine;

namespace Rector.UI.Graphs.Slots
{
    public static class SlotConverter
    {
        public static InputSlot Convert(NodeId nodeId, int index, IInput input, ReadOnlyReactiveProperty<bool> isMuted)
        {
            return input switch
            {
                ICallbackInput callbackInput => new CallbackInputSlot(nodeId, index, callbackInput.Name, callbackInput.Callback, isMuted),
                IFloatInput floatInput => new ReactivePropertyFloatInputSlot(nodeId, index, floatInput.Name, floatInput.Value, floatInput.DefaultValue, floatInput.MinValue, floatInput.MaxValue, isMuted),
                IIntInput intInput => new ReactivePropertyIntInputSlot(nodeId, index, intInput.Name, intInput.Value, intInput.DefaultValue, intInput.MinValue, intInput.MaxValue, isMuted),
                IBoolInput boolInput => new ReactivePropertyInputSlot<bool>(nodeId, index, boolInput.Name, boolInput.Value, boolInput.DefaultValue, isMuted),
                IVector3Input vector3Input => new ReactivePropertyInputSlot<Vector3>(nodeId, index, vector3Input.Name, vector3Input.Value, vector3Input.DefaultValue, isMuted),
                ITransformInput transformInput => new ReactivePropertyInputSlot<Transform>(nodeId, index, transformInput.Name, transformInput.Value, transformInput.DefaultValue, isMuted),
                _ => throw new System.NotImplementedException()
            };
        }

        public static OutputSlot Convert(NodeId nodeId, int index, IOutput output, ReadOnlyReactiveProperty<bool> isMuted)
        {
            return output switch
            {
                IObservableOutput<int> observableOutput => new ObservableOutputSlot<int>(nodeId, index, observableOutput.Name, observableOutput.Observable, isMuted),
                IObservableOutput<float> observableOutput => new ObservableOutputSlot<float>(nodeId, index, observableOutput.Name, observableOutput.Observable, isMuted),
                IObservableOutput<bool> observableOutput => new ObservableOutputSlot<bool>(nodeId, index, observableOutput.Name, observableOutput.Observable, isMuted),
                IObservableOutput<Vector3> observableOutput => new ObservableOutputSlot<Vector3>(nodeId, index, observableOutput.Name, observableOutput.Observable, isMuted),
                IObservableOutput<Transform> observableOutput => new ObservableOutputSlot<Transform>(nodeId, index, observableOutput.Name, observableOutput.Observable, isMuted),
                IObservableOutput<Unit> observableOutput => new ObservableOutputSlot<Unit>(nodeId, index, observableOutput.Name, observableOutput.Observable, isMuted),
                _ => throw new System.NotImplementedException()
            };
        }
    }
}
