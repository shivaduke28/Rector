using System.Collections.Generic;
using System.Linq;
using Unity.Pipeline.Commands;
using Unity.Profiling;
using UnityEngine;

namespace Rector.Cli
{
    /// <summary>
    /// issue #136 PoC — フレーム時間とシェーダーコンパイルの計測。
    ///
    /// CLI コマンドは同期の往復しかできないので begin / end で挟む。
    /// begin で DontDestroyOnLoad の MonoBehaviour を立てて毎フレーム
    /// unscaledDeltaTime を貯め、end で統計を返して破棄する。
    /// Shader.CreateGPUProgram の ProfilerRecorder は player で取れない可能性が
    /// あるため best-effort (Valid でなければ -1 を返す)。
    /// </summary>
    public static class FrameStatsRecorder
    {
        sealed class Runner : MonoBehaviour
        {
            public readonly List<float> Deltas = new(4096);
            public ProfilerRecorder CompileRecorder;
            public long CompileNs;
            public int CompileFrames;

            void Update()
            {
                Deltas.Add(Time.unscaledDeltaTime);
                if (CompileRecorder.Valid)
                {
                    var ns = CompileRecorder.LastValue;
                    if (ns > 0)
                    {
                        CompileNs += ns;
                        CompileFrames++;
                    }
                }
            }

            void OnDestroy() => CompileRecorder.Dispose();
        }

        static Runner runner;

        [CliCommand("rector_frame_stats_begin", "issue #136 PoC: start recording frame times (and shader compile time, best-effort).")]
        static object BeginCommand()
        {
            if (runner != null)
                return new { success = false, error = "already_recording", frames = runner.Deltas.Count };

            var go = new GameObject("FrameStatsRecorder (issue #136 PoC)");
            Object.DontDestroyOnLoad(go);
            runner = go.AddComponent<Runner>();
            runner.CompileRecorder = ProfilerRecorder.StartNew(ProfilerCategory.Render, "Shader.CreateGPUProgram");
            return new { success = true, compileRecorderValid = runner.CompileRecorder.Valid };
        }

        [CliCommand("rector_frame_stats_end", "issue #136 PoC: stop recording and return frame time stats.")]
        static object EndCommand()
        {
            if (runner == null)
                return new { success = false, error = "not_recording", message = "Call rector_frame_stats_begin first." };

            var deltas = runner.Deltas;
            var compileRecorderValid = runner.CompileRecorder.Valid;
            var compileMs = compileRecorderValid ? runner.CompileNs / 1_000_000.0 : -1;
            var compileFrames = compileRecorderValid ? runner.CompileFrames : -1;
            Object.Destroy(runner.gameObject);
            runner = null;

            if (deltas.Count == 0)
                return new { success = false, error = "no_frames" };

            var sorted = deltas.OrderBy(x => x).ToArray();
            return new
            {
                success = true,
                frames = sorted.Length,
                avgMs = sorted.Average() * 1000.0,
                maxMs = sorted[^1] * 1000.0,
                p95Ms = sorted[Mathf.Min(sorted.Length - 1, Mathf.FloorToInt(sorted.Length * 0.95f))] * 1000.0,
                framesOver33ms = sorted.Count(x => x > 0.033f),
                compileFrames,
                compileMs,
            };
        }
    }
}
