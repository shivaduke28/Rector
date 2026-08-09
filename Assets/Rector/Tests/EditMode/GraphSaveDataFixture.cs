using System.IO;
using Rector.UI.Graphs.Serialization;
using UnityEngine;

namespace Rector.Tests.EditMode
{
    /// <summary>保存データのテストで使う土台。形式のテストとスロットのテストで同じものを見る。</summary>
    public static class GraphSaveDataFixture
    {
        public const string SavedAtRaw = "2026-08-09T00:12:34.0000000+09:00";

        public static GraphSaveData Make()
        {
            return new GraphSaveData
            {
                version = GraphSaveData.CurrentVersion,
                savedAt = SavedAtRaw,
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

        public static string TempDirectory() =>
            Path.Combine(Path.GetTempPath(), "rector-graph-slot-tests", Path.GetRandomFileName());
    }
}
