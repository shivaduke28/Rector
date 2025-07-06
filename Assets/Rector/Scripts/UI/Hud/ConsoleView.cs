using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
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
            var cts = new CancellationTokenSource();
            var subscription = RectorLogger.Log.Subscribe(message => HandleAsync(message, cts.Token).Forget());
            
            return Disposable.Create(() =>
            {
                cts.Cancel();
                cts.Dispose();
                subscription.Dispose();
            });
        }

        async UniTaskVoid HandleAsync(string message, CancellationToken cancellationToken)
        {
            if (!poolLabels.TryDequeue(out var label))
            {
                var tree = VisualElementFactory.Instance.CreateConsoleLog();
                label = tree.Q<Label>();
            }

            label.text = message;
            visibleLabels.Enqueue(label);
            
            // Wait for next frame to avoid modifying hierarchy during layout calculation
            await UniTask.NextFrame(cancellationToken);
            
            consoleContent.Add(label);

            if (visibleLabels.Count > 7)
            {
                var oldLabel = visibleLabels.Dequeue();
                
                // Wait for next frame before removing
                await UniTask.NextFrame(cancellationToken);
                
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
