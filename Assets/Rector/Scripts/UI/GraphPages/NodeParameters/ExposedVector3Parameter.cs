using System.Collections.Generic;
using R3;
using Rector.UI.Graphs.Slots;
using UnityEngine;

namespace Rector.UI.GraphPages.NodeParameters
{
    public enum Vector3Axis
    {
        X = 0,
        Y = 1,
        Z = 2,
    }

    /// <summary>
    /// Vector3の入力スロット1本を、見出し1行 + X/Y/Zの3行として組む。
    /// </summary>
    public static class ExposedVector3Parameter
    {
        public static IEnumerable<IExposedRow> CreateRows(ReactivePropertyInputSlot<Vector3> slot,
            ReactiveProperty<SliderStepType> stepType)
        {
            yield return new ExposedVector3HeaderRow(slot.Name);
            yield return new ExposedVector3ComponentInputModel(slot, Vector3Axis.X, stepType);
            yield return new ExposedVector3ComponentInputModel(slot, Vector3Axis.Y, stepType);
            yield return new ExposedVector3ComponentInputModel(slot, Vector3Axis.Z, stepType);
        }
    }

    /// <summary>Vector3の見出し。名前を出すだけで、カーソルは止まらない。</summary>
    public sealed class ExposedVector3HeaderRow : IExposedRow
    {
        public string Label { get; }

        public ExposedVector3HeaderRow(string label)
        {
            Label = label;
        }
    }

    /// <summary>
    /// Vector3のX/Y/Zのうち1成分を担当する行。刻みはfloat行と共有する。
    /// </summary>
    /// <remarks>
    /// Vector3にはレンジが無いので刻みは整数から始まり、スライダーも出ない。
    /// 値の置き場はスロットのReactivePropertyだけで、成分の写しは持たない。
    /// </remarks>
    public sealed class ExposedVector3ComponentInputModel : IExposedInputModel
    {
        // レンジが無いぶん、刻みは ±1 -> ±0.1 -> ±0.01 と整数から始める。
        const int Digit = 0;

        public string Label { get; }
        public ReadOnlyReactiveProperty<Vector3> Value => slot.Property;
        public ReadOnlyReactiveProperty<SliderStepType> StepType => stepType;
        public ReactiveProperty<bool> IsFocused { get; } = new(false);

        readonly ReactivePropertyInputSlot<Vector3> slot;
        readonly int index;
        readonly ReactiveProperty<SliderStepType> stepType;

        public ExposedVector3ComponentInputModel(ReactivePropertyInputSlot<Vector3> slot, Vector3Axis axis,
            ReactiveProperty<SliderStepType> stepType)
        {
            this.slot = slot;
            this.stepType = stepType;
            index = (int)axis;
            Label = axis.ToString();
        }

        public string ValueFormat => ExposedStepCalculator.ValueFormat(Digit);

        public float StepSize(SliderStepType step) => ExposedStepCalculator.StepSize(Digit, step);

        /// <summary>Vector3から担当する成分だけを取り出す。</summary>
        public float Read(Vector3 value) => value[index];

        public void Increment() => Move(true);

        public void Decrement() => Move(false);

        void Move(bool increment)
        {
            var value = slot.Property.Value;
            value[index] = ExposedStepCalculator.Apply(value[index], Digit, stepType.CurrentValue, increment);
            slot.Property.Value = value;
        }

        /// <summary>刻み幅を x1 -> x10 -> x100 と回す。</summary>
        public void DoAction() => stepType.Value = ExposedStepCalculator.Next(stepType.CurrentValue);

        public void Focus() => IsFocused.Value = true;

        public void Unfocus() => IsFocused.Value = false;
    }
}
