using System;
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
    /// Vector3の入力スロット1本を、見出し1行 + X/Y/Zの3行として持つ。
    /// </summary>
    /// <remarks>
    /// 成分ごとの行はスロットを共有するので、刻み幅の桁と成分の写しはここで1度だけ用意する。
    /// 写しはスロットからの一方通行で、書き込みは行から直接スロットへ返す。
    /// 双方向にすると往復を止めるためのフラグが要る。
    /// </remarks>
    public sealed class ExposedVector3Parameter : IDisposable
    {
        public ExposedVector3HeaderRow Header { get; }
        public ExposedVector3ComponentInputModel[] Components { get; }

        readonly ReactiveProperty<float>[] components = new ReactiveProperty<float>[3];
        readonly IDisposable subscription;

        public ExposedVector3Parameter(ReactivePropertyVector3InputSlot slot, ReactiveProperty<SliderStepType> stepType)
        {
            var digit = ExposedStepCalculator.DigitFromRange(slot.MinValue, slot.MaxValue);
            var current = slot.Property.Value;
            for (var i = 0; i < components.Length; i++)
            {
                components[i] = new ReactiveProperty<float>(current[i]);
            }

            subscription = slot.Property.Subscribe(components, (value, cs) =>
            {
                for (var i = 0; i < cs.Length; i++) cs[i].Value = value[i];
            });

            Header = new ExposedVector3HeaderRow(slot.Name);
            Components = new[]
            {
                new ExposedVector3ComponentInputModel(slot, Vector3Axis.X, components[0], digit, stepType),
                new ExposedVector3ComponentInputModel(slot, Vector3Axis.Y, components[1], digit, stepType),
                new ExposedVector3ComponentInputModel(slot, Vector3Axis.Z, components[2], digit, stepType),
            };
        }

        public void Dispose()
        {
            subscription.Dispose();
            foreach (var c in components) c.Dispose();
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

    /// <summary>Vector3のX/Y/Zのうち1成分を担当する行。刻みはfloat行と共有する。</summary>
    public sealed class ExposedVector3ComponentInputModel : IExposedInputModel
    {
        public string Label { get; }
        public float MinValue => slot.MinValue;
        public float MaxValue => slot.MaxValue;

        /// <summary>スロットの値からこの成分だけを写したもの。</summary>
        public ReadOnlyReactiveProperty<float> Value => component;

        public ReadOnlyReactiveProperty<SliderStepType> StepType => stepType;
        public ReactiveProperty<bool> IsFocused { get; } = new(false);

        readonly ReactivePropertyVector3InputSlot slot;
        readonly int index;
        readonly ReactiveProperty<float> component;
        readonly ReactiveProperty<SliderStepType> stepType;
        readonly int digit;

        public ExposedVector3ComponentInputModel(ReactivePropertyVector3InputSlot slot, Vector3Axis axis,
            ReactiveProperty<float> component, int digit, ReactiveProperty<SliderStepType> stepType)
        {
            this.slot = slot;
            this.component = component;
            this.digit = digit;
            this.stepType = stepType;
            index = (int)axis;
            Label = axis.ToString();
        }

        public string ValueFormat => ExposedStepCalculator.ValueFormat(digit);

        public float StepSize(SliderStepType step) => ExposedStepCalculator.StepSize(digit, step);

        /// <summary>担当する成分だけを差し替えてスロットへ返す。</summary>
        public void Write(float value)
        {
            var v = slot.Property.Value;
            v[index] = Mathf.Clamp(value, slot.MinValue, slot.MaxValue);
            slot.Property.Value = v;
        }

        public void Increment() => Move(true);

        public void Decrement() => Move(false);

        void Move(bool increment)
        {
            Write(ExposedStepCalculator.Apply(
                component.Value, digit, stepType.CurrentValue, increment, slot.MinValue, slot.MaxValue));
        }

        /// <summary>刻み幅を x1 -> x10 -> x100 と回す。</summary>
        public void DoAction() => stepType.Value = ExposedStepCalculator.Next(stepType.CurrentValue);

        public void Focus() => IsFocused.Value = true;

        public void Unfocus() => IsFocused.Value = false;
    }
}
