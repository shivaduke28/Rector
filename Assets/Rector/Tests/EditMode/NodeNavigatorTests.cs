using System;
using System.Collections.Generic;
using NUnit.Framework;
using Rector.UI.GraphPages;
using Rector.UI.Graphs;
using Rector.UI.Graphs.Nodes;
using Rector.UI.Graphs.Slots;
using Rector.UI.LayeredGraphDrawing;
using UnityEngine;
using UnityEngine.UIElements;

namespace Rector.Tests.EditMode
{
    /// <summary>
    /// NodeNavigatorの空間ナビゲーションのテスト。
    /// GraphSorterは通さず、Sortが保証する不変条件(行はグループ順に連結)を満たす
    /// Layersを手で組み立てて検証する。
    /// </summary>
    public sealed class NodeNavigatorTests
    {
        // SetCountがPlayerPrefsへ書くので退避・復元する。保存キーはNodeGroupsの中の話なので
        // 触らず、NodeGroups自身に読み書きさせる(コンストラクタがclampするため、
        // 元の値が範囲外だったときは有効範囲内の値に戻る)
        int savedGroupCount;

        [SetUp]
        public void SetUp() => savedGroupCount = new NodeGroups().CurrentCount;

        [TearDown]
        public void TearDown() => new NodeGroups().SetCount(savedGroupCount);

        static readonly Vector2 Up = Vector2.up;
        static readonly Vector2 Down = Vector2.down;
        static readonly Vector2 Left = Vector2.left;
        static readonly Vector2 Right = Vector2.right;

        sealed class TestNode : Node
        {
            public TestNode(uint id) : base(new NodeId(id), $"N{id}")
            {
            }

            // 実在しない値にするとNodeViewのアイコン解決がVisualElementFactoryを触らずに済む
            public override NodeCategory Category => (NodeCategory)(-1);
            public override InputSlot[] InputSlots => Array.Empty<InputSlot>();
            public override OutputSlot[] OutputSlots => Array.Empty<OutputSlot>();
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

        static uint nextId;

        static LayeredNode CreateNode(int group, int layer, float x)
        {
            var view = new NodeView(CreateNodeTemplate(), new TestNode(nextId++));
            // Positionのsetterが、ナビゲーションの基準になるTargetPositionも確定させる
            view.Position = new Vector2(x, layer * 100f);
            return new LayeredNode(view) { Group = group, Layer = layer };
        }

        static LayeredGraph CreateGraph(params ILayeredNode[][] rows)
        {
            var graph = new LayeredGraph(new VisualElement(), new VisualElement());
            graph.Layers.Clear();
            foreach (var row in rows)
            {
                var list = new List<ILayeredNode>();
                foreach (var node in row)
                {
                    node.Index = list.Count;
                    list.Add(node);
                }

                graph.Layers.Add(list);
            }

            return graph;
        }

        static NodeGroups CreateGroups(int count)
        {
            var groups = new NodeGroups();
            groups.SetCount(count);
            return groups;
        }

        // issue-52の議論で使った基本形:
        //   A | B | D     A,C=グループ0 / B=グループ1 / D=グループ2
        //     | B |       レイヤーは A,D=0 / B=1 / C=2
        //   C |   |
        (NodeNavigator navigator, LayeredNode a, LayeredNode b, LayeredNode c, LayeredNode d) CreateKanbanFixture()
        {
            var groups = CreateGroups(3);
            var a = CreateNode(0, 0, 0f);
            var d = CreateNode(2, 0, 520f);
            var b = CreateNode(1, 1, 200f);
            var c = CreateNode(0, 2, 0f);
            var graph = CreateGraph(
                new ILayeredNode[] { a, d },
                new ILayeredNode[] { b },
                new ILayeredNode[] { c });
            return (new NodeNavigator(graph, groups), a, b, c, d);
        }

        [Test]
        public void Vertical_StaysInGroup_SkippingOtherGroupsBetweenLayers()
        {
            var (navigator, a, _, c, _) = CreateKanbanFixture();

            // AとCの間のレイヤーには別グループのBがいるが、上下はグループ内に閉じる
            Assert.That(navigator.SelectNextNode(a, Down), Is.SameAs(c));
            Assert.That(navigator.SelectNextNode(c, Up), Is.SameAs(a));
        }

        [Test]
        public void Vertical_WrapsWithinGroup()
        {
            var (navigator, a, _, c, _) = CreateKanbanFixture();

            Assert.That(navigator.SelectNextNode(c, Down), Is.SameAs(a));
            Assert.That(navigator.SelectNextNode(a, Up), Is.SameAs(c));
        }

        [Test]
        public void Vertical_AloneInGroup_FallsBackToNearestAcrossGroups()
        {
            var (navigator, a, b, c, d) = CreateKanbanFixture();

            // B(グループ1で唯一)の上下は他グループへ: 上はxが近いA、下はC
            Assert.That(navigator.SelectNextNode(b, Up), Is.SameAs(a));
            Assert.That(navigator.SelectNextNode(b, Down), Is.SameAs(c));
            // D(グループ2で唯一)の下は隣レイヤーのB
            Assert.That(navigator.SelectNextNode(d, Down), Is.SameAs(b));
        }

        [Test]
        public void Vertical_NoCandidateAnywhere_StaysPut()
        {
            var groups = CreateGroups(1);
            var a = CreateNode(0, 0, 0f);
            var graph = CreateGraph(new ILayeredNode[] { a });
            var navigator = new NodeNavigator(graph, groups);

            Assert.That(navigator.SelectNextNode(a, Down), Is.SameAs(a));
            Assert.That(navigator.SelectNextNode(a, Up), Is.SameAs(a));
        }

        [Test]
        public void Horizontal_WalksRowWithinGroup()
        {
            var groups = CreateGroups(1);
            var a = CreateNode(0, 0, 0f);
            var e = CreateNode(0, 0, 100f);
            var graph = CreateGraph(new ILayeredNode[] { a, e });
            var navigator = new NodeNavigator(graph, groups);

            Assert.That(navigator.SelectNextNode(a, Right), Is.SameAs(e));
            Assert.That(navigator.SelectNextNode(e, Left), Is.SameAs(a));
        }

        [Test]
        public void Horizontal_AtGroupEdge_EntersAdjacentGroupAtNearestNode()
        {
            var (navigator, a, b, _, d) = CreateKanbanFixture();

            // Aの行(レイヤー0)の右隣は行としてはDだが、まず隣のグループ1の最近傍Bに入る
            Assert.That(navigator.SelectNextNode(a, Right), Is.SameAs(b));
            Assert.That(navigator.SelectNextNode(b, Right), Is.SameAs(d));
        }

        [Test]
        public void Horizontal_WrapsAcrossGroups()
        {
            var (navigator, a, _, _, d) = CreateKanbanFixture();

            // 右端グループから右はグループ0へ戻る。レイヤーが近いAが選ばれる(Cではなく)
            Assert.That(navigator.SelectNextNode(d, Right), Is.SameAs(a));
            // 左端グループから左はグループ2へ回り込む
            Assert.That(navigator.SelectNextNode(a, Left), Is.SameAs(d));
        }

        [Test]
        public void Horizontal_SingleGroup_WrapsWithinRow()
        {
            var groups = CreateGroups(1);
            var a = CreateNode(0, 0, 0f);
            var e = CreateNode(0, 0, 100f);
            var graph = CreateGraph(new ILayeredNode[] { a, e });
            var navigator = new NodeNavigator(graph, groups);

            Assert.That(navigator.SelectNextNode(e, Right), Is.SameAs(a));
            Assert.That(navigator.SelectNextNode(a, Left), Is.SameAs(e));
        }

        [Test]
        public void Horizontal_SkipsDummyNodes()
        {
            var groups = CreateGroups(1);
            var a = CreateNode(0, 0, 0f);
            var dummy = new DummyNode(new NodeId(9999)) { Group = 0, Layer = 0 };
            var e = CreateNode(0, 0, 100f);
            var graph = CreateGraph(new ILayeredNode[] { a, dummy, e });
            var navigator = new NodeNavigator(graph, groups);

            Assert.That(navigator.SelectNextNode(a, Right), Is.SameAs(e));
            Assert.That(navigator.SelectNextNode(e, Left), Is.SameAs(a));
        }

        [Test]
        public void FindNodeInAdjacentGroup_SkipsEmptyGroupsAndWraps()
        {
            var groups = CreateGroups(3);
            var a = CreateNode(0, 0, 0f);
            var d = CreateNode(2, 0, 520f);
            var graph = CreateGraph(new ILayeredNode[] { a, d });
            var navigator = new NodeNavigator(graph, groups);

            // グループ1は空なのでスキップされ、右隣はグループ2のD
            Assert.That(navigator.FindNodeInAdjacentGroup(a, 1, groups.CurrentCount), Is.SameAs(d));
            // 右端からはグループ0へラップ
            Assert.That(navigator.FindNodeInAdjacentGroup(d, 1, groups.CurrentCount), Is.SameAs(a));
        }
    }
}
