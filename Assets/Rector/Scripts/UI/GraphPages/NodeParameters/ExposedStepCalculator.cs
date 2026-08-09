using UnityEngine;

namespace Rector.UI.GraphPages.NodeParameters
{
    /// <summary>
    /// 十字キーでの増減の刻み幅。float行とVector3の成分行が同じ計算を使う。
    /// </summary>
    public static class ExposedStepCalculator
    {
        // 刻みは10のべきで、桁は0..4しか来ない。Mathf.Powで毎回作ると誤差が乗るので表で持つ。
        static readonly float[] Scales = { 1f, 10f, 100f, 1000f, 10000f };
        static readonly float[] StepSizes = { 1f, 0.1f, 0.01f, 0.001f, 0.0001f };

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

        static int StepToDigit(int digit, SliderStepType step)
        {
            return step switch
            {
                SliderStepType.Times1 => digit,
                SliderStepType.Times10 => digit + 1,
                SliderStepType.Times100 => digit + 2,
                _ => digit
            };
        }

        public static float StepSize(int digit, SliderStepType step) => StepSizes[StepToDigit(digit, step)];

        /// <summary>刻みの格子に沿って1つ動かす。</summary>
        /// <remarks>
        /// 格子から外れた値は、進む向きの格子へ寄せるだけにする。丸めてから必ず1歩足す作りだと、
        /// 0.5で左を押したときに0を飛び越して-1まで動き、右と左で動く量も揃わない。
        ///
        /// 刻みを整数で数えているのは、小数位を指定して丸めると Math.Round(double, int) しか無く
        /// doubleへ広がるため。3.7fはdoubleでは3.70000004…になり、丸めた3.7とは別物に見えるので、
        /// 「まだ格子に寄せられる」と読んで1歩も動かなくなる。floatのまま整数で数えれば一致する。
        /// </remarks>
        public static float Apply(float value, int digit, SliderStepType step, bool increment)
        {
            var scale = Scales[StepToDigit(digit, step)];
            var units = Mathf.Round(value * scale);
            var grid = units / scale;
            if (increment) return grid > value ? grid : (units + 1f) / scale;
            return grid < value ? grid : (units - 1f) / scale;
        }

        public static float Apply(float value, int digit, SliderStepType step, bool increment, float minValue, float maxValue)
            => Mathf.Clamp(Apply(value, digit, step, increment), minValue, maxValue);

        /// <summary>刻み幅より2桁細かく表示する。</summary>
        public static string ValueFormat(int digit) => $"F{digit + 2}";

        /// <summary>フォーカス行に出す「±0.01」の表記。</summary>
        public static string StepLabel(float stepSize) => $"±{stepSize.ToString("0.####")}";

        /// <summary>刻み幅を x1 -> x10 -> x100 と回す。</summary>
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
