using System;
using UnityEngine;

namespace Rector.UI.GraphPages.NodeParameters
{
    /// <summary>
    /// 十字キーでの増減の刻み幅。float行とVector3の成分行が同じ計算を使う。
    /// </summary>
    public static class ExposedStepCalculator
    {
        /// <summary>
        /// レンジの広さから刻みの小数桁を決める。範囲を持たない入力は幅が Infinity になり整数刻みに落ちる。
        /// </summary>
        public static int DigitFromRange(float minValue, float maxValue)
        {
            return (maxValue - minValue) switch
            {
                >= 10f => 0, // 整数
                >= 1f => 1, // 小数第一位
                _ => 2 // 小数第二位
            };
        }

        public static int StepToDigit(int digit, SliderStepType step)
        {
            return step switch
            {
                SliderStepType.Times1 => digit,
                SliderStepType.Times10 => digit + 1,
                SliderStepType.Times100 => digit + 2,
                _ => digit
            };
        }

        public static float StepSize(int digit, SliderStepType step) => (float)Math.Pow(10, -StepToDigit(digit, step));

        /// <summary>刻みに丸めてから1ステップ動かす。</summary>
        public static float Apply(float value, int digit, SliderStepType step, bool increment, float minValue, float maxValue)
        {
            var d = StepToDigit(digit, step);
            var result = Math.Round(value, d);
            result += increment ? Math.Pow(10, -d) : -Math.Pow(10, -d);
            return Mathf.Clamp((float)result, minValue, maxValue);
        }

        /// <summary>刻み幅より2桁細かく表示する。</summary>
        public static string ValueFormat(int digit) => $"F{digit + 2}";

        /// <summary>フォーカス行に出す「±0.01」の表記。</summary>
        public static string StepLabel(float stepSize) => $"±{stepSize.ToString("0.####")}";

        /// <summary>刻み幅x1 -> x10 -> x100 と回す。</summary>
        public static SliderStepType Next(SliderStepType step)
        {
            return step switch
            {
                SliderStepType.Times1 => SliderStepType.Times10,
                SliderStepType.Times10 => SliderStepType.Times100,
                _ => SliderStepType.Times1
            };
        }
    }
}
