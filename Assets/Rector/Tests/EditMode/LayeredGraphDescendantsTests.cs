using System;
using System.Collections.Generic;
using NUnit.Framework;
using R3;
using Rector.UI.Graphs;
using Rector.UI.Graphs.Nodes;
using Rector.UI.Graphs.Slots;
using Rector.UI.LayeredGraphDrawing;
using UnityEngine;
using UnityEngine.UIElements;

namespace Rector.Tests.EditMode
{
    /// <summary>
    /// LayeredGraph.CollectDescendants の到達判定。
    /// GraphSorter は通さず、AddNode で登録したノードに Edge を手で積んで検証する。
    /// </summary>
    public sealed class LayeredGraphDescendantsTests
    {
        // NodeViewはアイコン解決でVisualElementFactoryに依存する。
        // ランタイムでRectorInstallerが行う初期化を、テストでは空のアセットで行う
        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            var settings = ScriptableObject.CreateInstance<Rector.UI.RectorUISettingsAsset>();
            settings.iconSettings = new Rector.UI.RectorIconSettings();
            Rector.UI.VisualElementFactory.Initialize(settings);
        }

        // スロットを持たせると NodeView がスロットの見た目を組もうとして空アセットで落ちるので、
        // スロットはノードの外で作って Edge にだけ渡す
        sealed class TestNode : Node
        {
            public TestNode(uint id) : base(new NodeId(id), $"N{id}")
            {
            }

            public override NodeCategory Category => NodeCategory.Operator;
            public override InputSlot[] InputSlots => Array.Empty<InputSlot>();
            public override OutputSlot[] OutputSlots => Array.Empty<OutputSlot>();
        }

        sealed class TestInputSlot : InputSlot<Unit>
        {
            public TestInputSlot(NodeId nodeId) : base(nodeId, 0, "in")
            {
            }

            public override void Send(Unit value)
            {
            }

            public override Observable<Unit> Observable() => R3.Observable.Empty<Unit>();
        }

        sealed class TestOutputSlot : OutputSlot<Unit>
        {
            public TestOutputSlot(NodeId nodeId) : base(nodeId, 0, "out")
            {
            }

            public override Observable<Unit> Observable() => R3.Observable.Empty<Unit>();
        }

        // NodeView.uxmlと同じ名前の要素だけを持つ最小のツリー
        static VisualElement CreateNodeTemplate()
        {
            var container = new VisualElement();
            var node = new VisualElement { name = "node" };
            node.Add(new Label { name = "name-label" });
            node.Add(new VisualElement { name = "input-slot-list" });
            node.Add(new VisualElement { name = "output-slot-list" });
            node.Add(new VisualElement { name = "icon" });
            container.Add(node);
            return container;
        }

        static VisualElement CreateSlotTemplate(string rootName)
        {
            var container = new VisualElement();
            var root = new VisualElement { name = rootName };
            root.Add(new Label { name = "name-label" });
            container.Add(root);
            return container;
        }

        static uint nextId;

        static LayeredNode AddNode(LayeredGraph graph)
        {
            var view = new NodeView(CreateNodeTemplate(), new TestNode(nextId++));
            graph.AddNode(view, 0);
            Assert.That(graph.TryGetNode(view.Node.Id, out var node), Is.True);
            return node;
        }

        /// <summary>parent → child のエッジを、Sort が見る EdgesToChild / EdgesToParent に積む。</summary>
        static void Connect(LayeredNode parent, LayeredNode child)
        {
            var output = new TestOutputSlot(parent.Id);
            var input = new TestInputSlot(child.Id);
            var edge = new Edge(output, input, Disposable.Empty);
            var view = new EdgeView(
                new OutputSlotView(CreateSlotTemplate("output-slot")),
                new InputSlotView(CreateSlotTemplate("input-slot")),
                edge);
            var layeredEdge = new LayeredEdge(view);
            parent.EdgesToChild.Add(layeredEdge);
            child.EdgesToParent.Add(layeredEdge);
        }

        static HashSet<LayeredNode> Collect(LayeredGraph graph, LayeredNode root)
        {
            var result = new HashSet<LayeredNode>();
            graph.CollectDescendants(root, result);
            return result;
        }

        static LayeredGraph CreateGraph() => new(new VisualElement(), new VisualElement());

        [Test]
        public void ReachesEveryDepth_AndExcludesRoot()
        {
            // A → B → C, A → D
            var graph = CreateGraph();
            var a = AddNode(graph);
            var b = AddNode(graph);
            var c = AddNode(graph);
            var d = AddNode(graph);
            Connect(a, b);
            Connect(b, c);
            Connect(a, d);

            Assert.That(Collect(graph, a), Is.EquivalentTo(new[] { b, c, d }));
            Assert.That(Collect(graph, b), Is.EquivalentTo(new[] { c }));
        }

        [Test]
        public void DiamondIsCollectedOnce()
        {
            // A → B → D, A → C → D
            var graph = CreateGraph();
            var a = AddNode(graph);
            var b = AddNode(graph);
            var c = AddNode(graph);
            var d = AddNode(graph);
            Connect(a, b);
            Connect(a, c);
            Connect(b, d);
            Connect(c, d);

            Assert.That(Collect(graph, a), Is.EquivalentTo(new[] { b, c, d }));
        }

        [Test]
        public void DoesNotClimbToParents_OrSiblings()
        {
            // P → A, P → S。A から見て P(親) と S(兄弟) は子孫ではない
            var graph = CreateGraph();
            var p = AddNode(graph);
            var a = AddNode(graph);
            var s = AddNode(graph);
            Connect(p, a);
            Connect(p, s);

            Assert.That(Collect(graph, a), Is.Empty);
        }

        /// <remarks>ValidateLoop が弾いているが、CLI 等から万一入っても無限ループにならないこと。</remarks>
        [Test]
        public void CycleTerminates()
        {
            var graph = CreateGraph();
            var a = AddNode(graph);
            var b = AddNode(graph);
            Connect(a, b);
            Connect(b, a);

            Assert.That(Collect(graph, a), Is.EquivalentTo(new[] { b }));
        }
    }
}
