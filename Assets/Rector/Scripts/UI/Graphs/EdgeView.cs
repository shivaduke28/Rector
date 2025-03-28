using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using R3;
using Rector.UI.Nodes;

namespace Rector.UI.Graphs
{
    public sealed class EdgeView : VisualElement, IDisposable
    {
        readonly OutputSlotView output;
        readonly InputSlotView input;
        public List<DummyNode> DummyNodes { get; } = new();

        public Edge Edge { get; }
        readonly IDisposable disposable;

        public EdgeView(OutputSlotView output, InputSlotView input, Edge edge)
        {
            this.output = output;
            this.input = input;
            this.Edge = edge;
            generateVisualContent += OnGenerateVisualContent;
            pickingMode = PickingMode.Ignore;
            style.position = Position.Absolute;
            style.left = 0f;
            style.top = 0f;
            disposable = Observable.EveryValueChanged(output, s => s.ConnectorPosition).DistinctUntilChanged()
                .Merge(Observable.EveryValueChanged(input, s => s.ConnectorPosition).DistinctUntilChanged())
                .AsUnitObservable()
                .Subscribe(_ => Repaint());
        }


        public void Repaint()
        {
            MarkDirtyRepaint();
        }

        void OnGenerateVisualContent(MeshGenerationContext context)
        {
            var painter = context.painter2D;

            painter.strokeColor = new Color(1, 1, 1, 0.1f);
            painter.lineWidth = 1f;
            painter.BeginPath();

            var startPoint = parent.WorldToLocal(output.ConnectorPosition);
            var endPoint = parent.WorldToLocal(input.ConnectorPosition);

            foreach (var dummy in DummyNodes)
            {
                var dummyPoint = dummy.Position + new Vector2(0, 15f);
                DrawLine(startPoint, dummyPoint, painter);
                startPoint = dummyPoint;
            }

            DrawLine(startPoint, endPoint, painter);
            painter.Stroke();
            return;

            static void DrawLine(Vector2 start, Vector2 end, Painter2D painter)
            {
                painter.MoveTo(start);
                painter.LineTo(end);
            }
        }


        public void Dispose()
        {
            disposable.Dispose();
        }
    }
}
