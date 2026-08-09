using System;
using System.Globalization;
using System.IO;
using System.Text.RegularExpressions;
using NUnit.Framework;
using Rector.UI.Graphs.Serialization;
using UnityEngine;
using UnityEngine.TestTools;

namespace Rector.Tests.EditMode
{
    /// <summary>
    /// 保存スロットへの読み書きと削除。ファイルの内容そのものは <see cref="GraphSaveDataTests"/>。
    /// </summary>
    public sealed class GraphSlotRepositoryTests
    {
        static string TempDirectory() => GraphSaveDataFixture.TempDirectory();

        static GraphSaveData MakeData() => GraphSaveDataFixture.Make();

        [Test]
        public void WritesAndReadsBackASlot()
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
                // FormatSavedAt はオフセット付きの保存値をローカル時刻へ直して見せるので、
                // 表示文字列を固定すると JST 以外で落ちる。保存した瞬間に戻ることだけ見る
                var shown = DateTime.Parse(info.SavedAt, CultureInfo.InvariantCulture);
                var expected = DateTimeOffset.Parse(GraphSaveDataFixture.SavedAtRaw, CultureInfo.InvariantCulture).LocalDateTime;
                Assert.That(shown, Is.EqualTo(new DateTime(expected.Year, expected.Month, expected.Day, expected.Hour, expected.Minute, 0)));
            }
            finally
            {
                if (Directory.Exists(directory)) Directory.Delete(directory, true);
            }
        }

        [Test]
        public void OverwritesAnExistingSlot()
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
        public void RefusesAnUnsupportedVersion()
        {
            var directory = TempDirectory();
            try
            {
                Directory.CreateDirectory(directory);
                File.WriteAllText(Path.Combine(directory, "slot1.json"), "{\"version\":99,\"nodes\":[],\"edges\":[]}");

                LogAssert.Expect(LogType.Error, new Regex("unsupported version"));
                Assert.That(new GraphSlotRepository(directory).Read(1), Is.Null);
            }
            finally
            {
                if (Directory.Exists(directory)) Directory.Delete(directory, true);
            }
        }

        [Test]
        public void ReportsUnwrittenSlotsAsEmpty()
        {
            var repository = new GraphSlotRepository(TempDirectory());

            Assert.That(repository.Read(1), Is.Null);

            var info = repository.GetInfo(1);
            Assert.That(info.IsEmpty, Is.True);
            Assert.That(info.Number, Is.EqualTo(1));
            Assert.That(info.SavedAt, Is.Empty);
        }

        [Test]
        public void RefusesSlotsOutOfRange()
        {
            var repository = new GraphSlotRepository(TempDirectory());

            Assert.That(GraphSlotRepository.IsValidSlot(0), Is.False);
            Assert.That(GraphSlotRepository.IsValidSlot(1), Is.True);
            Assert.That(GraphSlotRepository.IsValidSlot(GraphSlotRepository.SlotCount), Is.True);
            Assert.That(GraphSlotRepository.IsValidSlot(GraphSlotRepository.SlotCount + 1), Is.False);

            Assert.That(repository.Write(0, MakeData()), Is.False);
            Assert.That(repository.Read(0), Is.Null);
            Assert.That(repository.Delete(0), Is.False);
            Assert.That(repository.Delete(GraphSlotRepository.SlotCount + 1), Is.False);
        }

        [Test]
        public void ListsEverySlot()
        {
            var infos = new GraphSlotRepository(TempDirectory()).GetAllInfo();

            Assert.That(infos.Length, Is.EqualTo(GraphSlotRepository.SlotCount));
            Assert.That(infos[0].Number, Is.EqualTo(1));
            Assert.That(infos[GraphSlotRepository.SlotCount - 1].Number, Is.EqualTo(GraphSlotRepository.SlotCount));
        }

        [Test]
        public void DeleteEmptiesTheSlotAndLeavesTheOthers()
        {
            var directory = TempDirectory();
            try
            {
                var repository = new GraphSlotRepository(directory);
                repository.Write(1, MakeData());
                repository.Write(2, MakeData());

                Assert.That(repository.Delete(1), Is.True);

                Assert.That(repository.Read(1), Is.Null);
                Assert.That(repository.GetInfo(1).IsEmpty, Is.True);
                Assert.That(repository.GetInfo(2).IsEmpty, Is.False);
            }
            finally
            {
                if (Directory.Exists(directory)) Directory.Delete(directory, true);
            }
        }

        /// <remarks>「消えている状態にする」操作なので、元から空でも成功。空かどうかは呼ぶ側が見る。</remarks>
        [Test]
        public void DeleteIsIdempotent()
        {
            var directory = TempDirectory();
            try
            {
                var repository = new GraphSlotRepository(directory);

                Assert.That(repository.Delete(1), Is.True);

                repository.Write(1, MakeData());
                Assert.That(repository.Delete(1), Is.True);
                Assert.That(repository.Delete(1), Is.True);
            }
            finally
            {
                if (Directory.Exists(directory)) Directory.Delete(directory, true);
            }
        }
    }
}
