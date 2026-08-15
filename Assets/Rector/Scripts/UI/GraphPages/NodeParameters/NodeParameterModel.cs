using System.Collections.Generic;
using R3;
using Rector.UI.Graphs.Nodes;
using Rector.UI.Graphs.Slots;
using UnityEngine;

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
        readonly ReactiveProperty<SliderStepType> stepType = new(SliderStepType.Times1);
        public readonly ReactiveProperty<bool> IsVisible = new(false);

        /// <summary>Viewが上から順に並べる行。見出しのように操作を持たない行も含む。</summary>
        public IReadOnlyList<IExposedRow> Rows => rows;

        readonly List<IExposedRow> rows = new();

        /// <summary>カーソルが止まる行。<see cref="rows"/> から導出するので取りこぼしが起きない。</summary>
        readonly List<IExposedInputModel> focusableRows = new();

        int index = -1;

        public NodeParameterModel(GraphPage page)
        {
            this.page = page;
        }

        public void Enter()
        {
            Clear();

            if (page.SelectedNode is { } selectedNode)
            {
                foreach (var inputSlot in selectedNode.NodeView.Node.InputSlots)
                {
                    switch (inputSlot)
                    {
                        case ReactivePropertyFloatInputSlot floatInputSlot:
                            rows.Add(new ExposedFloatInputModel(floatInputSlot, stepType));
                            break;
                        case ReactivePropertyIntInputSlot intInputSlot:
                            rows.Add(new ExposedIntInputModel(intInputSlot));
                            break;
                        case ReactivePropertyInputSlot<Vector3> vector3InputSlot:
                            rows.AddRange(ExposedVector3Parameter.CreateRows(vector3InputSlot, stepType));
                            break;
                        case ReactivePropertyInputSlot<bool> boolInputSlot:
                            rows.Add(new ExposedBoolInputModel(boolInputSlot));
                            break;
                        case CallbackFloatInputSlot callbackFloatInputSlot:
                            rows.Add(new ExposedCallbackFloatInputModel(callbackFloatInputSlot, stepType));
                            break;
                        case CallbackInputSlot callbackInputSlot:
                            rows.Add(new ExposedCallbackInputModel(callbackInputSlot));
                            break;
                    }
                }
            }

            foreach (var row in rows)
            {
                if (row is IExposedInputModel focusable) focusableRows.Add(focusable);
            }

            // 先頭の操作できる行にフォーカスを置く。操作できる行が無いノードでは-1のまま。
            index = focusableRows.Count > 0 ? 0 : -1;
            if (index >= 0) focusableRows[index].Focus();

            IsVisible.Value = true;
        }

        void Clear()
        {
            rows.Clear();
            focusableRows.Clear();
            index = -1;
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

            focusableRows[index].Unfocus();
            index = (index + (next ? 1 : -1) + focusableRows.Count) % focusableRows.Count;
            focusableRows[index].Focus();
        }

        public void Increment()
        {
            if (index == -1) return;
            focusableRows[index].Increment();
        }

        public void Decrement()
        {
            if (index == -1) return;
            focusableRows[index].Decrement();
        }

        public void DoAction()
        {
            if (index == -1) return;
            focusableRows[index].DoAction();
        }
    }
}
