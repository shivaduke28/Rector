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
                // グループはスロットではないがどのノードにもあるので先頭に置く
                ExposedInputs.Add(new ExposedGroupInputModel(page, selectedNode));

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

            // 先頭はGroup行なので、パラメータを持つノードでは2番目から始める。
            // ここを0のままにすると、開いて最初の十字キー左右がノードのグループ移動になってしまう。
            index = ExposedInputs.Count switch
            {
                0 => -1,
                1 => 0,
                _ => 1
            };
            if (index >= 0) ExposedInputs[index].Focus();

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
            ExposedInputs[index].Increment();
        }

        public void Decrement()
        {
            if (index == -1) return;
            ExposedInputs[index].Decrement();
        }

        public void DoAction()
        {
            if (index == -1) return;
            ExposedInputs[index].DoAction();
        }
    }
}
