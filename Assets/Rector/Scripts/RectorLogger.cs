using System;
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
