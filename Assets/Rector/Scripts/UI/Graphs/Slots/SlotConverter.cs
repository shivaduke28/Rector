using R3;
using Rector.Nodes;
using UnityEngine;

namespace Rector.UI.Graphs.Slots
{
    public static class SlotConverter
    {
        public static InputSlot Convert(NodeId nodeId, int index, IInput input, ReadOnlyReactiveProperty<bool> isMuted)
        {
            return input switch
            {
                CallbackInput callbackInput => new CallbackInputSlot(nodeId, index, callbackInput.Name, callbackInput.Callback, isMuted),
                FloatInput floatInput => new ReactivePropertyFloatInputSlot(nodeId, index, floatInput.Name, floatInput.Value, floatInput.DefaultValue, floatInput.MinValue, floatInput.MaxValue, isMuted),
                IntInput intInput => new ReactivePropertyIntInputSlot(nodeId, index, intInput.Name, intInput.Value, intInput.DefaultValue, intInput.MinValue, intInput.MaxValue, isMuted),
                BoolInput boolInput => new ReactivePropertyInputSlot<bool>(nodeId, index, boolInput.Name, boolInput.Value, boolInput.DefaultValue, isMuted),
                Vector3Input vector3Input => new ReactivePropertyInputSlot<Vector3>(nodeId, index, vector3Input.Name, vector3Input.Value, vector3Input.DefaultValue, isMuted),
                TransformInput transformInput => new ReactivePropertyInputSlot<Transform>(nodeId, index, transformInput.Name, transformInput.Value, transformInput.DefaultValue, isMuted),
                _ => throw new System.NotImplementedException()
            };
        }

        public static OutputSlot Convert(NodeId nodeId, int index, IOutput output, ReadOnlyReactiveProperty<bool> isMuted)
        {
            return output switch
            {
                ObservableOutput<int> observableOutput => new ObservableOutputSlot<int>(nodeId, index, observableOutput.Name, observableOutput.Observable, isMuted),
                ObservableOutput<float> observableOutput => new ObservableOutputSlot<float>(nodeId, index, observableOutput.Name, observableOutput.Observable, isMuted),
                ObservableOutput<bool> observableOutput => new ObservableOutputSlot<bool>(nodeId, index, observableOutput.Name, observableOutput.Observable, isMuted),
                ObservableOutput<Vector3> observableOutput => new ObservableOutputSlot<Vector3>(nodeId, index, observableOutput.Name, observableOutput.Observable, isMuted),
                ObservableOutput<Transform> observableOutput => new ObservableOutputSlot<Transform>(nodeId, index, observableOutput.Name, observableOutput.Observable, isMuted),
                ObservableOutput<Unit> observableOutput => new ObservableOutputSlot<Unit>(nodeId, index, observableOutput.Name, observableOutput.Observable, isMuted),
                _ => throw new System.NotImplementedException()
            };
        }
    }
}
