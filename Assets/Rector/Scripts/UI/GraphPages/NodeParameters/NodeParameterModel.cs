using System.Collections.Generic;
using R3;
using Rector.UI.Graphs.Nodes;
using Rector.UI.Graphs.Slots;

namespace Rector.UI.GraphPages.NodeParameters
{
    public enum SliderStepType
    {
        Times1,
        Times10,
        Times100,
    }

    public sealed class NodeParameterModel
    {
        public Node Node => page.SelectedNode?.NodeView.Node;
        readonly GraphPage page;
        public readonly ReactiveProperty<bool> IsVisible = new(false);
        public readonly List<IExposedInputModel> ExposedInputs = new();
        readonly ReactiveProperty<SliderStepType> stepType = new(SliderStepType.Times1);
        int index = 0;

        public NodeParameterModel(GraphPage page)
        {
            this.page = page;
        }

        public void Enter()
        {
            ExposedInputs.Clear();
            if (page.SelectedNode is { } selectedNode)
            {
                // カラムはスロットではないがどのノードにもあるので先頭に置く
                ExposedInputs.Add(new ExposedColumnInputModel(page, selectedNode));

                foreach (var inputSlot in selectedNode.NodeView.Node.InputSlots)
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

        /// <summary>
        /// 表示を消すだけ。State は動かさないので GraphPage の State 購読から呼べる。
        /// </summary>
        public void Hide()
        {
            if (!IsVisible.Value) return;
            IsVisible.Value = false;
        }

        public void Close()
        {
            Hide();
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
                case ExposedColumnInputModel columnInputViewModel:
                    columnInputViewModel.Increment();
                    break;
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
                case ExposedColumnInputModel columnInputViewModel:
                    columnInputViewModel.Decrement();
                    break;
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
