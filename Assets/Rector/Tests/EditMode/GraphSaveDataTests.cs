using System;
using NUnit.Framework;
using Rector.UI.Graphs;
using Rector.UI.Graphs.Nodes;
using Rector.UI.Graphs.Serialization;
using UnityEngine;

namespace Rector.Tests.EditMode
{
    /// <summary>
    /// グラフ保存データの形式。プリセットへの読み書きは <see cref="GraphPresetRepositoryTests"/>。
    /// グラフの組み立て自体は VisualElement に依存するのでここでは扱わず、CLI で確認する。
    /// </summary>
    public sealed class GraphSaveDataTests
    {
        static GraphSaveData MakeData() => GraphSaveDataFixture.Make();

        [Test]
        public void JsonRoundTripPreservesEverything()
        {
            var restored = JsonUtility.FromJson<GraphSaveData>(JsonUtility.ToJson(MakeData()));

            Assert.That(restored.version, Is.EqualTo(GraphSaveData.CurrentVersion));
            Assert.That(restored.savedAt, Is.EqualTo("2026-08-09T00:12:34.0000000+09:00"));
            Assert.That(restored.nodes.Length, Is.EqualTo(2));
            Assert.That(restored.nodes[0].nodeType, Is.EqualTo("MidiCcNode"));
            Assert.That(restored.nodes[0].group, Is.EqualTo(0));
            Assert.That(restored.nodes[1].group, Is.EqualTo(3));
            Assert.That(restored.nodes[0].ints[0].index, Is.EqualTo(1));
            Assert.That(restored.nodes[0].ints[0].value, Is.EqualTo(42));
            Assert.That(restored.nodes[0].bools[0].value, Is.True);
            Assert.That(restored.nodes[1].floats[0].value, Is.EqualTo(0.25f));
            Assert.That(restored.nodes[1].vector3s[0].value, Is.EqualTo(new Vector3(1, 2, 3)));
            Assert.That(restored.edges.Length, Is.EqualTo(1));
            Assert.That(restored.edges[0].fromType, Is.EqualTo("Float"));
            Assert.That(restored.edges[0].toType, Is.EqualTo("Float"));
        }

        /// <remarks>
        /// 配列フィールドの初期化子が効いていることの確認。これが崩れると、キーの無い JSON で
        /// 配列が null になり復元側の foreach が落ちる。
        /// group はキーが無ければ 0(先頭グループ)。group を持たない古いファイルが従来通りに読める根拠。
        /// </remarks>
        [Test]
        public void MissingArrayKeysStayEmptyRatherThanNull()
        {
            var restored = JsonUtility.FromJson<GraphSaveData>("{\"version\":1,\"nodes\":[{\"nodeType\":\"FloatNode\"}]}");

            Assert.That(restored.nodes[0].group, Is.EqualTo(0));
            Assert.That(restored.edges, Is.Not.Null.And.Empty);
            Assert.That(restored.nodes[0].floats, Is.Not.Null.And.Empty);
            Assert.That(restored.nodes[0].ints, Is.Not.Null.And.Empty);
            Assert.That(restored.nodes[0].bools, Is.Not.Null.And.Empty);
            Assert.That(restored.nodes[0].vector3s, Is.Not.Null.And.Empty);
        }

        /// <remarks>version を持たない JSON が現行版として通ってしまわないこと。</remarks>
        [Test]
        public void VersionIsNotSupportedUnlessWritten()
        {
            Assert.That(JsonUtility.FromJson<GraphSaveData>("{\"nodes\":[]}").IsSupportedVersion, Is.False);
            Assert.That(JsonUtility.FromJson<GraphSaveData>("{\"version\":99}").IsSupportedVersion, Is.False);
            Assert.That(MakeData().IsSupportedVersion, Is.True);
        }

        // ------------------------------------------------------------ テンプレートID

        [Test]
        public void CodeTemplateIdIsTheNodeClassName()
        {
            var id = NodeTemplateId.Code<FloatNode>();

            Assert.That(id.Kind, Is.EqualTo(NodeTemplateKind.Code));
            Assert.That(id.TypeName, Is.EqualTo("FloatNode"));
            Assert.That(id, Is.EqualTo(NodeTemplateId.Code("FloatNode")));
            Assert.That(id, Is.Not.EqualTo(NodeTemplateId.Code<SinNode>()));
        }

        [Test]
        public void BehaviourTemplateIdIsTheGuid()
        {
            var guid = Guid.NewGuid();

            Assert.That(NodeTemplateId.Behaviour(guid).Kind, Is.EqualTo(NodeTemplateKind.Behaviour));
            Assert.That(NodeTemplateId.Behaviour(guid), Is.EqualTo(NodeTemplateId.Behaviour(guid)));
            Assert.That(NodeTemplateId.Behaviour(guid), Is.Not.EqualTo(NodeTemplateId.Behaviour(Guid.NewGuid())));
        }

        /// <remarks>同じ名前でも種類が違えば別のテンプレート。直和が潰れていないことの確認。</remarks>
        [Test]
        public void DefaultTemplateIdIsInvalid()
        {
            Assert.That(default(NodeTemplateId).IsValid, Is.False);
            Assert.That(NodeTemplateId.Code<FloatNode>().IsValid, Is.True);
            Assert.That(NodeTemplateId.Behaviour(Guid.NewGuid()).IsValid, Is.True);
        }

        [Test]
        public void TemplateIdSurvivesTheSaveDataRoundTrip()
        {
            var guid = Guid.NewGuid();
            foreach (var id in new[] { NodeTemplateId.Code<FloatNode>(), NodeTemplateId.Behaviour(guid) })
            {
                var data = new NodeSaveData();
                NodeTemplateIdSaveData.Write(id, data);
                var json = JsonUtility.FromJson<NodeSaveData>(JsonUtility.ToJson(data));

                Assert.That(NodeTemplateIdSaveData.Read(json), Is.EqualTo(id));
            }
        }

        /// <remarks>片方のフィールドしか埋まらないこと。直和を平たく書いているので取り違えないため。</remarks>
        [Test]
        public void OnlyTheFieldForTheKindIsWritten()
        {
            var code = new NodeSaveData();
            NodeTemplateIdSaveData.Write(NodeTemplateId.Code<FloatNode>(), code);
            Assert.That(code.templateKind, Is.EqualTo("Code"));
            Assert.That(code.nodeType, Is.EqualTo("FloatNode"));
            Assert.That(code.behaviourGuid, Is.Empty);

            var behaviour = new NodeSaveData();
            NodeTemplateIdSaveData.Write(NodeTemplateId.Behaviour(Guid.NewGuid()), behaviour);
            Assert.That(behaviour.templateKind, Is.EqualTo("Behaviour"));
            Assert.That(behaviour.nodeType, Is.Empty);
            Assert.That(behaviour.behaviourGuid, Is.Not.Empty);
        }

        [Test]
        public void UnreadableTemplateIdIsInvalidRatherThanGuessed()
        {
            Assert.That(NodeTemplateIdSaveData.Read(new NodeSaveData()).IsValid, Is.False);
            Assert.That(NodeTemplateIdSaveData.Read(new NodeSaveData { templateKind = "Nonsense" }).IsValid, Is.False);
            Assert.That(NodeTemplateIdSaveData.Read(new NodeSaveData { templateKind = "Code" }).IsValid, Is.False);
            Assert.That(NodeTemplateIdSaveData.Read(new NodeSaveData { templateKind = "Behaviour", behaviourGuid = "not-a-guid" }).IsValid, Is.False);
            // 種類と中身が食い違うもの
            Assert.That(NodeTemplateIdSaveData.Read(new NodeSaveData { templateKind = "Behaviour", nodeType = "FloatNode" }).IsValid, Is.False);
        }
    }
}
