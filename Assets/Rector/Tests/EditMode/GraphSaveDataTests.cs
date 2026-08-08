using System;
using System.IO;
using NUnit.Framework;
using Rector.UI.Graphs;
using Rector.UI.Graphs.Nodes;
using Rector.UI.Graphs.Serialization;
using UnityEngine;
using UnityEngine.TestTools;

namespace Rector.Tests.EditMode
{
    /// <summary>
    /// グラフ保存データの形式と、スロットへの読み書き。
    /// グラフの組み立て自体は VisualElement に依存するのでここでは扱わず、CLI で確認する。
    /// </summary>
    public sealed class GraphSaveDataTests
    {
        static GraphSaveData MakeData()
        {
            return new GraphSaveData
            {
                version = GraphSaveData.CurrentVersion,
                savedAt = "2026-08-09T00:12:34.0000000+09:00",
                nodes = new[]
                {
                    new NodeSaveData
                    {
                        templateKind = "Code", nodeType = "MidiCcNode",
                        ints = new[] { new IntSlotValue { index = 1, value = 42 } },
                        bools = new[] { new BoolSlotValue { index = 0, value = true } },
                    },
                    new NodeSaveData
                    {
                        templateKind = "Code", nodeType = "FloatNode",
                        floats = new[] { new FloatSlotValue { index = 0, value = 0.25f } },
                        vector3s = new[] { new Vector3SlotValue { index = 3, value = new Vector3(1, 2, 3) } },
                    },
                },
                edges = new[]
                {
                    new EdgeSaveData
                    {
                        fromNode = 0, fromSlot = 0, fromType = "Float",
                        toNode = 1, toSlot = 0, toType = "Float",
                    },
                },
            };
        }

        [Test]
        public void JsonRoundTripPreservesEverything()
        {
            var restored = JsonUtility.FromJson<GraphSaveData>(JsonUtility.ToJson(MakeData()));

            Assert.That(restored.version, Is.EqualTo(GraphSaveData.CurrentVersion));
            Assert.That(restored.savedAt, Is.EqualTo("2026-08-09T00:12:34.0000000+09:00"));
            Assert.That(restored.nodes.Length, Is.EqualTo(2));
            Assert.That(restored.nodes[0].nodeType, Is.EqualTo("MidiCcNode"));
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
        /// </remarks>
        [Test]
        public void MissingArrayKeysStayEmptyRatherThanNull()
        {
            var restored = JsonUtility.FromJson<GraphSaveData>("{\"version\":1,\"nodes\":[{\"nodeType\":\"FloatNode\"}]}");

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

        // ------------------------------------------------------------------- スロット保存

        static string TempDirectory() =>
            Path.Combine(Path.GetTempPath(), "rector-graph-slot-tests", Path.GetRandomFileName());

        [Test]
        public void RepositoryWritesAndReadsBackASlot()
        {
            var directory = TempDirectory();
            try
            {
                var repository = new GraphSlotRepository(directory);
                Assert.That(repository.Write(1, MakeData()), Is.True);

                var read = repository.Read(1);
                Assert.That(read, Is.Not.Null);
                Assert.That(read.nodes.Length, Is.EqualTo(2));
                Assert.That(read.nodes[0].ints[0].value, Is.EqualTo(42));

                var info = repository.GetInfo(1);
                Assert.That(info.IsEmpty, Is.False);
                Assert.That(info.NodeCount, Is.EqualTo(2));
                Assert.That(info.EdgeCount, Is.EqualTo(1));
                Assert.That(info.SavedAt, Is.EqualTo("2026-08-09 00:12"));
            }
            finally
            {
                if (Directory.Exists(directory)) Directory.Delete(directory, true);
            }
        }

        [Test]
        public void RepositoryOverwritesAnExistingSlot()
        {
            var directory = TempDirectory();
            try
            {
                var repository = new GraphSlotRepository(directory);
                repository.Write(1, MakeData());

                var second = MakeData();
                second.nodes = new[] { new NodeSaveData { templateKind = "Code", nodeType = "FloatNode" } };
                Assert.That(repository.Write(1, second), Is.True);

                Assert.That(repository.Read(1).nodes.Length, Is.EqualTo(1));
                Assert.That(Directory.GetFiles(directory, "*.tmp"), Is.Empty);
            }
            finally
            {
                if (Directory.Exists(directory)) Directory.Delete(directory, true);
            }
        }

        [Test]
        public void RepositoryRefusesAnUnsupportedVersion()
        {
            var directory = TempDirectory();
            try
            {
                Directory.CreateDirectory(directory);
                File.WriteAllText(Path.Combine(directory, "slot1.json"), "{\"version\":99,\"nodes\":[],\"edges\":[]}");

                LogAssert.Expect(LogType.Error, new System.Text.RegularExpressions.Regex("unsupported version"));
                Assert.That(new GraphSlotRepository(directory).Read(1), Is.Null);
            }
            finally
            {
                if (Directory.Exists(directory)) Directory.Delete(directory, true);
            }
        }

        [Test]
        public void RepositoryReportsUnwrittenSlotsAsEmpty()
        {
            var repository = new GraphSlotRepository(TempDirectory());

            Assert.That(repository.Read(1), Is.Null);

            var info = repository.GetInfo(1);
            Assert.That(info.IsEmpty, Is.True);
            Assert.That(info.Number, Is.EqualTo(1));
            Assert.That(info.SavedAt, Is.Empty);
        }

        [Test]
        public void RepositoryRefusesSlotsOutOfRange()
        {
            var repository = new GraphSlotRepository(TempDirectory());

            Assert.That(GraphSlotRepository.IsValidSlot(0), Is.False);
            Assert.That(GraphSlotRepository.IsValidSlot(1), Is.True);
            Assert.That(GraphSlotRepository.IsValidSlot(GraphSlotRepository.SlotCount), Is.True);
            Assert.That(GraphSlotRepository.IsValidSlot(GraphSlotRepository.SlotCount + 1), Is.False);

            Assert.That(repository.Write(0, MakeData()), Is.False);
            Assert.That(repository.Read(0), Is.Null);
        }

        [Test]
        public void RepositoryListsEverySlot()
        {
            var infos = new GraphSlotRepository(TempDirectory()).GetAllInfo();

            Assert.That(infos.Length, Is.EqualTo(GraphSlotRepository.SlotCount));
            Assert.That(infos[0].Number, Is.EqualTo(1));
            Assert.That(infos[GraphSlotRepository.SlotCount - 1].Number, Is.EqualTo(GraphSlotRepository.SlotCount));
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
