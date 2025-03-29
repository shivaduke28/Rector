using System.Collections.Generic;
using R3;
using Rector.UI.Graphs.Nodes;
using Rector.UI.NodeEdit;

namespace Rector.UI.GraphPages
{
    public enum SliderStepType
    {
        Times1,
        Times10,
        Times100,
    }

    public sealed class NodeDetailModel
    {
        public Node Node => page.SelectedNode.Value;
        readonly GraphPage page;
        public readonly ReactiveProperty<bool> IsVisible = new(false);
        public readonly List<IExposedInputModel> ExposedInputs = new();
        readonly ReactiveProperty<SliderStepType> stepType = new(SliderStepType.Times1);
        int index = 0;

        public NodeDetailModel(GraphPage page)
        {
            this.page = page;
        }

        public void Enter()
        {
            ExposedInputs.Clear();
            if (Node != null)
            {
                foreach (var inputSlot in Node.InputSlots)
                {
                    switch (inputSlot)
                    {
                        case ReactivePropertyFloatInputSlot floatInputSlot:
                            ExposedInputs.Add(new ExposedFloatInputModel(floatInputSlot, stepType));
                            break;
                        case ReactivePropertyIntInputSlot intInputSlot:
                            ExposedInputs.Add(new ExposedIntInputModel(intInputSlot));
                            break;
                        case ReactivePropertyInputSlot<bool> boolInputSlot:
                            ExposedInputs.Add(new ExposedBoolInputModel(boolInputSlot));
                            break;
                        case CallbackInputSlot callbackInputSlot:
                            ExposedInputs.Add(new ExposedCallbackInputModel(callbackInputSlot));
                            break;
                    }
                }
            }

            if (ExposedInputs.Count > 0)
            {
                index = 0;
                ExposedInputs[index].Focus();
            }
            else
            {
                index = -1;
            }

            IsVisible.Value = true;
        }

        public void Close()
        {
            IsVisible.Value = false;
            page.State.Value = GraphPageState.NodeSelection;
        }

        public void Navigate(bool next)
        {
            if (index == -1) return;

            ExposedInputs[index].Unfocus();
            index = (index + (next ? 1 : -1) + ExposedInputs.Count) % ExposedInputs.Count;
            ExposedInputs[index].Focus();
        }

        public void Increment()
        {
            if (index == -1) return;
            var input = ExposedInputs[index];
            switch (input)
            {
                case ExposedFloatInputModel floatInputViewModel:
                    floatInputViewModel.Increment();
                    break;
                case ExposedIntInputModel intInputViewModel:
                    intInputViewModel.Increment();
                    break;
                case ExposedBoolInputModel boolInputViewModel:
                    boolInputViewModel.Set(true);
                    break;
            }
        }

        public void Decrement()
        {
            if (index == -1) return;
            var input = ExposedInputs[index];
            switch (input)
            {
                case ExposedFloatInputModel floatInputViewModel:
                    floatInputViewModel.Decrement();
                    break;
                case ExposedIntInputModel intInputViewModel:
                    intInputViewModel.Decrement();
                    break;
                case ExposedBoolInputModel boolInputViewModel:
                    boolInputViewModel.Set(false);
                    break;
            }
        }

        public void DoAction()
        {
            if (index == -1) return;
            var input = ExposedInputs[index];
            switch (input)
            {
                case ExposedFloatInputModel:
                    stepType.Value = stepType.CurrentValue switch
                    {
                        SliderStepType.Times1 => SliderStepType.Times10,
                        SliderStepType.Times10 => SliderStepType.Times100,
                        _ => SliderStepType.Times1
                    };
                    break;
                case ExposedBoolInputModel boolInputViewModel:
                    boolInputViewModel.Toggle();
                    break;
                case ExposedCallbackInputModel callbackInputViewModel:
                    callbackInputViewModel.Invoke();
                    break;
            }
        }
    }
}
