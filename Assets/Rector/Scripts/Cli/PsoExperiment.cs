using System.Diagnostics;
using System.IO;
using Unity.Pipeline.Commands;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using Debug = UnityEngine.Debug;

namespace Rector.Cli
{
    /// <summary>
    /// issue #136 PoC — GraphicsStateCollection による PSO トレースとウォームアップの実験。
    ///
    /// development build で rector_pso_trace_begin → コンテンツを網羅再生 →
    /// rector_pso_trace_end で .graphicsstate を保存し、次回起動時に
    /// TryWarmupAtStartup が同じファイルを読んでウォームアップする。
    /// A/B 比較はファイルの有無だけで切り替える（フラグや設定は持たない）。
    ///
    /// 実験の結論が出たら削除するか、本実装 (別 issue) に昇格する。
    /// </summary>
    public static class PsoExperiment
    {
        static GraphicsStateCollection trace;

        // API ごとに別コレクションが必要 (Metal / D3D11 / D3D12 で互換がない)
        static string FilePath => Path.Combine(
            Application.persistentDataPath, "PsoTrace",
            $"{SystemInfo.graphicsDeviceType}.graphicsstate");

        /// <summary>
        /// RectorInstaller が最初の BG シーンをロードする直前 (LoadingView 表示中) に呼ぶ。
        /// トレースファイルが無ければ何もしない。PoC なので同期で待ち、所要時間をログに残す。
        /// </summary>
        public static void TryWarmupAtStartup()
        {
            var path = FilePath;
            if (!File.Exists(path))
            {
                Debug.Log($"[PSO] no trace file at {path}, cold start");
                return;
            }

            var sw = Stopwatch.StartNew();
            var collection = new GraphicsStateCollection();
            if (!collection.LoadFromFile(path))
            {
                Debug.LogWarning($"[PSO] failed to load {path}");
                return;
            }

            collection.WarmUp().Complete();
            Debug.Log($"[PSO] warmup: {collection.variantCount} variants / {collection.totalGraphicsStateCount} states in {sw.ElapsedMilliseconds} ms");
        }

        [CliCommand("rector_pso_trace_begin", "issue #136 PoC: start tracing PSOs into a GraphicsStateCollection. Development build only.")]
        static object TraceBeginCommand()
        {
            trace ??= new GraphicsStateCollection();
            trace.BeginTrace();
            return new { success = trace.isTracing, isTracing = trace.isTracing };
        }

        [CliCommand("rector_pso_trace_end", "issue #136 PoC: stop tracing and save the .graphicsstate file.")]
        static object TraceEndCommand()
        {
            if (trace == null || !trace.isTracing)
                return new { success = false, error = "not_tracing", message = "Call rector_pso_trace_begin first." };

            trace.EndTrace();
            var path = FilePath;
            Directory.CreateDirectory(Path.GetDirectoryName(path)!);
            var saved = trace.SaveToFile(path);
            return new
            {
                success = saved,
                path,
                variants = trace.variantCount,
                states = trace.totalGraphicsStateCount,
            };
        }

        [CliCommand("rector_pso_warmup", "issue #136 PoC: load the saved .graphicsstate and warm up now (blocking).")]
        static object WarmupCommand()
        {
            var path = FilePath;
            if (!File.Exists(path))
                return new { success = false, error = "no_trace_file", path };

            var sw = Stopwatch.StartNew();
            var collection = new GraphicsStateCollection();
            if (!collection.LoadFromFile(path))
                return new { success = false, error = "load_failed", path };

            collection.WarmUp().Complete();
            return new
            {
                success = true,
                variants = collection.variantCount,
                states = collection.totalGraphicsStateCount,
                elapsedMs = sw.ElapsedMilliseconds,
            };
        }

        [CliCommand("rector_pso_status", "issue #136 PoC: tracing state, trace file path and graphics API.")]
        static object StatusCommand() => new
        {
            isTracing = trace?.isTracing ?? false,
            fileExists = File.Exists(FilePath),
            path = FilePath,
            api = SystemInfo.graphicsDeviceType.ToString(),
            isDebugBuild = Debug.isDebugBuild,
        };
    }
}
