using System;
using System.Linq;
using R3;
using Rector.UI.Graphs;
using Rector.UI.Graphs.Nodes;
using UnityEngine;

namespace Rector
{
    public static class RectorLogger
    {
        static readonly Subject<string> LogSubject = new();
        public static Observable<string> Log => LogSubject;

        public static void WelcomeMessage()
        {
            LogInternal("[SYSTEM/GREETING] Welcome to Rector!");
        }

        public static IDisposable SubscribeDebugLog()
        {
            Application.logMessageReceived += OnLogMessageReceived;
            return Disposable.Create(() => Application.logMessageReceived -= OnLogMessageReceived);
        }

        static void OnLogMessageReceived(string condition, string stacktrace, LogType type)
        {
            LogInternal($"[UNITY/{type.ToString().ToUpper()}] {condition}");
        }

        public static void AudioInputDevice(string id, string deviceName)
        {
            LogInternal($"[SYSTEM/AUDIO] {deviceName}");
        }

        public static void Resolution(int width, int height, FullScreenMode mode)
        {
            LogInternal($"[SYSTEM/DISPLAY] {width}x{height} {mode}");
        }

        public static void MidiInputDevice(string product, int channel, bool connected)
        {
            // channel は Minis の 0 始まりを MIDI 慣習の 1 始まりで表示する
            LogInternal($"[SYSTEM/MIDI] {(connected ? "connected" : "disconnected")} device='{product}' ch={channel + 1}");
        }

        public static void MidiInputDeviceSelection(string portName, bool selected)
        {
            LogInternal($"[SYSTEM/MIDI] {(selected ? "selected" : "deselected")} device='{portName}'");
        }

        public static void MidiInputIgnored(string portName)
        {
            LogInternal($"[SYSTEM/MIDI] ignored input from '{portName}'. Enable it in System > MIDI Settings.");
        }

        // MIDI と OSC で共通のラーン。タグに MIDI を残すと OSC ノードのログが嘘になる
        public static void SourceNodeLearn(Node node, string status)
        {
            LogInternal($"[NODE/LEARN] id={node.Id} name='{node.Name}' {status}");
        }

        // 受信は IPAddress.Any だが、送信側には宛先を打ち込む必要がある。
        // 会場でこの行を読むだけで iPad の設定を埋められるようにしておく
        public static void OscListening(int port, string[] localAddresses)
        {
            var targets = localAddresses.Length > 0
                ? string.Join(", ", localAddresses.Select(a => $"{a}:{port}"))
                : $"127.0.0.1:{port}";
            LogInternal($"[SYSTEM/OSC] listening on port {port}. Send to {targets}");
        }

        public static void OscDisabled()
        {
            LogInternal("[SYSTEM/OSC] input is off. Enable it in System > OSC Settings.");
        }

        public static void OscBindFailed(int port, string message)
        {
            LogInternal($"[SYSTEM/OSC] failed to listen on port {port}. {message}");
        }

        public static void OscInputOverflow()
        {
            LogInternal("[SYSTEM/OSC] dropped incoming messages. The receive queue overflowed.");
        }

        public static void DisableStackTrace()
        {
            Application.SetStackTraceLogType(LogType.Log, StackTraceLogType.None);
        }

        public static void CreateNode(Node node)
        {
            LogInternal($"[NODE/CREATE] id={node.Id} name='{node.Name}'");
        }

        public static void DeleteNode(Node node)
        {
            LogInternal($"[NODE/DELETE] id={node.Id} name='{node.Name}'");
        }

        public static void CreateEdge(Edge edge, Node output, Node input)
        {
            LogInternal($"[EDGE/CREATE] src=({output.Id},{output.Name},{edge.OutputSlot.Name}) dst=({input.Id},{input.Name},{edge.InputSlot.Name})");
        }

        public static void DeleteEdge(Edge edge, Node output, Node input)
        {
            LogInternal($"[EDGE/DELETE] src=({output.Id},{output.Name},{edge.OutputSlot.Name}) dst=({input.Id},{input.Name},{edge.InputSlot.Name})");
        }

        public static void ToggleMute(Node node, bool mute)
        {
            if (mute)
            {
                LogInternal($"[NODE/MUTE] id={node.Id} name='{node.Name}'");
            }
            else
            {
                LogInternal($"[NODE/UNMUTE] id={node.Id} name='{node.Name}'");
            }
        }

        public static void GraphSaved(int slot, int nodeCount, int edgeCount, int skippedNodes, int skippedEdges)
        {
            var skipped = skippedNodes > 0 || skippedEdges > 0
                ? $" (skipped {skippedNodes} scene node(s), {skippedEdges} edge(s))"
                : "";
            LogInternal($"[GRAPH/SAVE] slot={slot} nodes={nodeCount} edges={edgeCount}{skipped}");
        }

        public static void GraphLoaded(int slot, int nodeCount, int edgeCount, int skippedNodes, int skippedEdges)
        {
            var skipped = skippedNodes > 0 || skippedEdges > 0
                ? $" (dropped {skippedNodes} node(s), {skippedEdges} edge(s))"
                : "";
            LogInternal($"[GRAPH/LOAD] slot={slot} added {nodeCount} node(s), {edgeCount} edge(s){skipped}");
        }

        public static void GraphSlotDeleted(int slot)
        {
            LogInternal($"[GRAPH/DELETE] slot={slot}");
        }

        public static void GraphCleared(int nodeCount)
        {
            LogInternal($"[GRAPH/CLEAR] removed {nodeCount} node(s)");
        }

        public static void GraphSlotEmpty(int slot)
        {
            LogInternal($"[GRAPH/LOAD] slot={slot} is empty.");
        }

        public static void GraphLoadSkippedNode(string saveKey)
        {
            LogInternal($"[GRAPH/LOAD] unknown node template '{saveKey}'. Skipped.");
        }

        public static void GraphLoadSkippedValue(Node node, int slotIndex, string savedType)
        {
            LogInternal($"[GRAPH/LOAD] value dropped: {node.Name}[{slotIndex}] is not a {savedType} input");
        }

        public static void GraphLoadSkippedEdge(string reason)
        {
            LogInternal($"[GRAPH/LOAD] edge dropped: {reason}");
        }

        public static void ActiveCamera(string cameraName)
        {
            LogInternal($"[CAMERA/CHANGE] name='{cameraName}'");
        }

        public static void LoopDetected(NodeId outputNodeId, NodeId inputNodeId)
        {
            // エラー用のstyleあてたい
            LogInternal($"[EDGE/ERROR] Loop detected. src={outputNodeId} dst={inputNodeId}");
        }

        static string TimeString()
        {
            var time = Time.realtimeSinceStartup;
            var h = Mathf.FloorToInt(time / (60 * 60));
            var min = Mathf.FloorToInt((time / 60) % 60);
            var sec = time % 60;
            return $"{h:00}:{min:00}:{sec:00}";
        }

        static void LogInternal(string message)
        {
            LogSubject.OnNext($"[{TimeString()}] {message}");
        }

    }
}
