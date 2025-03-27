using System;
using System.Collections.Generic;
using R3;
using UnityEngine;
using UnityEngine.UIElements;

namespace Rector.UI.Hud
{
    public sealed class ConsoleView
    {
        readonly VisualElement consoleContent;
        readonly Queue<Label> visibleLabels = new();
        readonly Queue<Label> poolLabels = new();
        bool translated;

        public ConsoleView(VisualElement root)
        {
            consoleContent = root.Query<VisualElement>("console-content");
        }

        public IDisposable Bind()
        {
            return RectorLogger.Log.Subscribe(Handle);
        }

        void Handle(string message)
        {
            if (!poolLabels.TryDequeue(out var label))
            {
                var tree = VisualElementFactory.Instance.CreateConsoleLog();
                label = tree.Q<Label>();
            }

            label.text = message;
            visibleLabels.Enqueue(label);
            consoleContent.Add(label);

            if (visibleLabels.Count > 7)
            {
                var oldLabel = visibleLabels.Dequeue();
                consoleContent.Remove(oldLabel);
                poolLabels.Enqueue(oldLabel);
                if (!translated)
                {
                    // consoleContent.transform.position = new Vector3(0, 6, 0);
                    translated = true;
                }
            }
        }
    }
}
