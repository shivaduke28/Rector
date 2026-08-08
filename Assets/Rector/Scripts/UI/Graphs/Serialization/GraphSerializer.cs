using System;
using System.Collections.Generic;
using System.Globalization;
using Rector.UI.GraphPages;
using Rector.UI.Graphs.Nodes;
using Rector.UI.Graphs.Slots;
using Rector.UI.LayeredGraphDrawing;
using UnityEngine;

#nullable enable

namespace Rector.UI.Graphs.Serialization
{
    /// <summary>
    /// 保存・読み込みの結果。何を運べて何が落ちたかを呼び出し側へ返す。
    /// </summary>
    public readonly struct GraphTransferResult
    {
        public readonly int NodeCount;
        public readonly int EdgeCount;
        public readonly int SkippedNodeCount;
        public readonly int SkippedEdgeCount;

        public GraphTransferResult(int nodeCount, int edgeCount, int skippedNodeCount, int skippedEdgeCount)
        {
            NodeCount = nodeCount;
            EdgeCount = edgeCount;
            SkippedNodeCount = skippedNodeCount;
            SkippedEdgeCount = skippedEdgeCount;
        }
    }

    /// <summary>
    /// グラフと GraphSaveData の間の変換。
    /// </summary>
    public sealed class GraphSerializer
    {
        readonly GraphPage graphPage;
        readonly NodeTemplateRepository nodeTemplateRepository;

        public GraphSerializer(GraphPage graphPage, NodeTemplateRepository nodeTemplateRepository)
        {
            this.graphPage = graphPage;
            this.nodeTemplateRepository = nodeTemplateRepository;
        }

        // ---------------------------------------------------------------- 保存

        public GraphSaveData Capture(out GraphTransferResult result)
        {
            var graph = graphPage.Graph;

            // 保存できないノード(BGシーン由来)はエッジの端点にもなれないので、
            // 先に「保存するノード」を確定してから index を振る。
            var indexOf = new Dictionary<NodeId, int>();
            var nodes = new List<NodeSaveData>();
            var skippedNodeCount = 0;

            foreach (var layeredNode in graph.Nodes)
            {
                var node = layeredNode.NodeView.Node;
                if (!node.IsSaveable)
                {
                    skippedNodeCount++;
                    continue;
                }

                indexOf.Add(node.Id, nodes.Count);
                nodes.Add(CaptureNode(node));
            }

            var edges = new List<EdgeSaveData>();
            var skippedEdgeCount = 0;

            foreach (var id in graph.Edges.Keys)
            {
                if (!indexOf.TryGetValue(id.OutputNodeId, out var from) ||
                    !indexOf.TryGetValue(id.InputNodeId, out var to))
                {
                    skippedEdgeCount++;
                    continue;
                }

                edges.Add(new EdgeSaveData
                {
                    fromNode = from,
                    fromSlot = id.OutputSlotIndex,
                    fromType = OutputTypeName(graph, id.OutputNodeId, id.OutputSlotIndex),
                    toNode = to,
                    toSlot = id.InputSlotIndex,
                    toType = InputTypeName(graph, id.InputNodeId, id.InputSlotIndex),
                });
            }

            result = new GraphTransferResult(nodes.Count, edges.Count, skippedNodeCount, skippedEdgeCount);

            return new GraphSaveData
            {
                version = GraphSaveData.CurrentVersion,
                savedAt = DateTimeOffset.Now.ToString("o", CultureInfo.InvariantCulture),
                nodes = nodes.ToArray(),
                edges = edges.ToArray(),
            };
        }

        static NodeSaveData CaptureNode(Node node)
        {
            var floats = new List<FloatSlotValue>();
            var ints = new List<IntSlotValue>();
            var bools = new List<BoolSlotValue>();
            var vector3s = new List<Vector3SlotValue>();

            foreach (var slot in node.InputSlots)
            {
                // 繋がっているスロットはロード後にエッジが値を流し込むので保存しない。
                // CallbackInputSlot(イベント) と Transform 入力はそもそも値を持たない/持ち出せない。
                if (slot.ConnectedCount > 0) continue;

                switch (slot)
                {
                    case ReactivePropertyInputSlot<float> s:
                        floats.Add(new FloatSlotValue { index = s.Index, value = s.Property.Value });
                        break;
                    case ReactivePropertyInputSlot<int> s:
                        ints.Add(new IntSlotValue { index = s.Index, value = s.Property.Value });
                        break;
                    case ReactivePropertyInputSlot<bool> s:
                        bools.Add(new BoolSlotValue { index = s.Index, value = s.Property.Value });
                        break;
                    case ReactivePropertyInputSlot<Vector3> s:
                        vector3s.Add(new Vector3SlotValue { index = s.Index, value = s.Property.Value });
                        break;
                }
            }

            var data = new NodeSaveData
            {
                floats = floats.ToArray(),
                ints = ints.ToArray(),
                bools = bools.ToArray(),
                vector3s = vector3s.ToArray(),
            };
            NodeTemplateIdSaveData.Write(node.TemplateId, data);
            return data;
        }

        static string OutputTypeName(LayeredGraph graph, NodeId nodeId, int index) =>
            graph.TryGetNode(nodeId, out var n) ? TypeNameAt(n.NodeView.Node.OutputSlots, index) : "";

        static string InputTypeName(LayeredGraph graph, NodeId nodeId, int index) =>
            graph.TryGetNode(nodeId, out var n) ? TypeNameAt(n.NodeView.Node.InputSlots, index) : "";

        static string TypeNameAt<T>(T[] slots, int index) where T : ISlot =>
            index >= 0 && index < slots.Length ? slots[index].Type.ToString() : "";

        // -------------------------------------------------------------- ロード

        /// <summary>
        /// 保存データのノードとエッジを今のグラフへ足す。復元できないものは飛ばして残りを組む。
        /// </summary>
        /// <remarks>
        /// 既存のノードには触らない(非破壊)。ノードIDは採番し直し、エッジは保存ファイル内の
        /// index から引くので、読み込んだノード同士しか繋がらない。既存グラフと混線することはない。
        /// 差し込み先のグループは AddNode が決める(選択中のノードと同じグループ)ので、
        /// メニューを開く前に選んでおいたノードが行き先になる。
        /// 丸ごと入れ替えたいときは、呼ぶ側が先に GraphPage.ClearGraph を通すこと。
        /// </remarks>
        public GraphTransferResult Restore(GraphSaveData data)
        {
            var nodeIds = new NodeId?[data.nodes.Length];
            var skippedNodeCount = 0;
            NodeId? firstNodeId = null;

            for (var i = 0; i < data.nodes.Length; i++)
            {
                var saved = data.nodes[i];
                var templateId = NodeTemplateIdSaveData.Read(saved);
                if (!templateId.IsValid || !nodeTemplateRepository.TryGet(templateId, out var template))
                {
                    RectorLogger.GraphLoadSkippedNode(templateId.IsValid ? templateId.ToString() : $"{saved.templateKind}/{saved.nodeType}{saved.behaviourGuid}");
                    skippedNodeCount++;
                    continue;
                }

                var nodeView = template.Create(NodeId.Generate());
                graphPage.AddNode(nodeView);
                nodeIds[i] = nodeView.Node.Id;
                firstNodeId ??= nodeView.Node.Id;

                // エッジより先に値を入れる。接続すると上流の現在値が流れ込むので、
                // 順序を逆にすると復元した値がいったん上書きされる過渡状態が生まれる。
                RestoreValues(nodeView.Node, saved);
            }

            var edgeCount = 0;
            var skippedEdgeCount = 0;

            foreach (var edge in data.edges)
            {
                if (TryConnect(edge, nodeIds)) edgeCount++;
                else skippedEdgeCount++;
            }

            SelectFirstLoadedNode(firstNodeId);

            graphPage.Sort();
            return new GraphTransferResult(data.nodes.Length - skippedNodeCount, edgeCount, skippedNodeCount, skippedEdgeCount);
        }

        /// <remarks>
        /// index の位置に同じ型の入力スロットが無ければ、その値は捨てる。
        /// ノードのスロット構成が変わった古いファイルで、関係ないスロットへ値を入れないため。
        /// </remarks>
        static void RestoreValues(Node node, NodeSaveData saved)
        {
            foreach (var v in saved.floats)
            {
                if (ValueSlotAt<float>(node, v.index) is { } slot) slot.Property.Value = v.value;
                else RectorLogger.GraphLoadSkippedValue(node, v.index, "Float");
            }

            foreach (var v in saved.ints)
            {
                if (ValueSlotAt<int>(node, v.index) is { } slot) slot.Property.Value = v.value;
                else RectorLogger.GraphLoadSkippedValue(node, v.index, "Int");
            }

            foreach (var v in saved.bools)
            {
                if (ValueSlotAt<bool>(node, v.index) is { } slot) slot.Property.Value = v.value;
                else RectorLogger.GraphLoadSkippedValue(node, v.index, "Boolean");
            }

            foreach (var v in saved.vector3s)
            {
                if (ValueSlotAt<Vector3>(node, v.index) is { } slot) slot.Property.Value = v.value;
                else RectorLogger.GraphLoadSkippedValue(node, v.index, "Vector3");
            }
        }

        static ReactivePropertyInputSlot<T>? ValueSlotAt<T>(Node node, int index) =>
            SlotAt(node.InputSlots, index) as ReactivePropertyInputSlot<T>;

        static T? SlotAt<T>(T[] slots, int index) where T : class, ISlot =>
            index >= 0 && index < slots.Length ? slots[index] : null;

        /// <summary>
        /// 差し込んだ先頭のノードへカーソルを移す。メニューを閉じたら、そのまま繋ぎにいける。
        /// </summary>
        /// <remarks>
        /// 全部足し終えてから呼ぶこと。途中で選択を動かすと、以降の AddNode の
        /// 行き先グループが変わってしまう。
        /// </remarks>
        void SelectFirstLoadedNode(NodeId? firstNodeId)
        {
            if (firstNodeId is not { } id) return;
            if (!graphPage.Graph.TryGetNode(id, out var first)) return;

            graphPage.EnterNodeSelection(first);
        }

        bool TryConnect(EdgeSaveData edge, NodeId?[] nodeIds)
        {
            if (!TryGetNode(edge.fromNode, nodeIds, out var from) ||
                !TryGetNode(edge.toNode, nodeIds, out var to))
            {
                return false;
            }

            var output = SlotAt(from.OutputSlots, edge.fromSlot);
            var input = SlotAt(to.InputSlots, edge.toSlot);
            if (output == null || input == null)
            {
                RectorLogger.GraphLoadSkippedEdge($"slot index out of range ({from.Name}[{edge.fromSlot}] -> {to.Name}[{edge.toSlot}])");
                return false;
            }

            // 型が食い違うなら、スロットの並びが変わったファイル。別のスロットへ繋がないよう落とす
            if (output.Type.ToString() != edge.fromType || input.Type.ToString() != edge.toType)
            {
                RectorLogger.GraphLoadSkippedEdge(
                    $"slot type changed ({from.Name}[{edge.fromSlot}] {edge.fromType}->{output.Type}, {to.Name}[{edge.toSlot}] {edge.toType}->{input.Type})");
                return false;
            }

            var result = graphPage.TryConnectSlots(output, input);
            if (result == ConnectResult.Connected) return true;

            RectorLogger.GraphLoadSkippedEdge($"{result} ({from.Name}.{output.Name} -> {to.Name}.{input.Name})");
            return false;
        }

        bool TryGetNode(int savedIndex, NodeId?[] nodeIds, out Node node)
        {
            node = null!;
            if (savedIndex < 0 || savedIndex >= nodeIds.Length) return false;
            if (nodeIds[savedIndex] is not { } id) return false;
            if (!graphPage.Graph.TryGetNode(id, out var layeredNode)) return false;

            node = layeredNode.NodeView.Node;
            return true;
        }
    }
}
