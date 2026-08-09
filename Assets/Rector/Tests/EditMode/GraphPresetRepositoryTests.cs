using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using NUnit.Framework;
using Rector.UI.Graphs.Serialization;
using UnityEngine;
using UnityEngine.TestTools;

namespace Rector.Tests.EditMode
{
    /// <summary>
    /// プリセットへの読み書きと削除、そして一覧。ファイルの内容そのものは <see cref="GraphSaveDataTests"/>。
    /// </summary>
    public sealed class GraphPresetRepositoryTests
    {
        static string TempDirectory() => GraphSaveDataFixture.TempDirectory();

        static GraphSaveData MakeData() => GraphSaveDataFixture.Make();

        [Test]
        public void WritesAndReadsBackAPreset()
        {
            var directory = TempDirectory();
            try
            {
                var repository = new GraphPresetRepository(directory);
                Assert.That(repository.Write("ambient drift", MakeData()), Is.True);

                var read = repository.Read("ambient drift");
                Assert.That(read, Is.Not.Null);
                Assert.That(read.nodes.Length, Is.EqualTo(2));
                Assert.That(read.nodes[0].ints[0].value, Is.EqualTo(42));

                Assert.That(repository.TryGetInfo("ambient drift", out var info), Is.True);
                Assert.That(info.Name, Is.EqualTo("ambient drift"));
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
        public void OverwritesAnExistingPreset()
        {
            var directory = TempDirectory();
            try
            {
                var repository = new GraphPresetRepository(directory);
                repository.Write("a", MakeData());

                var second = MakeData();
                second.nodes = new[] { new NodeSaveData { templateKind = "Code", nodeType = "FloatNode" } };
                Assert.That(repository.Write("a", second), Is.True);

                Assert.That(repository.Read("a").nodes.Length, Is.EqualTo(1));
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
                File.WriteAllText(Path.Combine(directory, "a.json"), "{\"version\":99,\"nodes\":[],\"edges\":[]}");

                LogAssert.Expect(LogType.Error, new Regex("unsupported version"));
                Assert.That(new GraphPresetRepository(directory).Read("a"), Is.Null);
            }
            finally
            {
                if (Directory.Exists(directory)) Directory.Delete(directory, true);
            }
        }

        [Test]
        public void ReportsMissingPresets()
        {
            var repository = new GraphPresetRepository(TempDirectory());

            Assert.That(repository.Read("nope"), Is.Null);
            Assert.That(repository.Exists("nope"), Is.False);
            Assert.That(repository.TryGetInfo("nope", out _), Is.False);
        }

        [Test]
        public void ListsNothingWhenTheDirectoryIsMissing()
        {
            Assert.That(new GraphPresetRepository(TempDirectory()).GetAll(), Is.Empty);
        }

        [Test]
        public void ListsPresetsSortedByName()
        {
            var directory = TempDirectory();
            try
            {
                var repository = new GraphPresetRepository(directory);
                repository.Write("charlie", MakeData());
                repository.Write("alpha", MakeData());
                repository.Write("Bravo", MakeData());

                Assert.That(repository.GetAll().Select(x => x.Name), Is.EqualTo(new[] { "alpha", "Bravo", "charlie" }));
            }
            finally
            {
                if (Directory.Exists(directory)) Directory.Delete(directory, true);
            }
        }

        /// <remarks>
        /// 一覧はディスクにあるものを見るので、アプリが書いたとは限らないファイルが混ざる。
        /// 読めないものは黙って飛ばす。ここでログを出すと、開くたびにHUDのコンソールが埋まる。
        /// </remarks>
        [Test]
        public void ListSkipsFilesItCannotRead()
        {
            var directory = TempDirectory();
            try
            {
                var repository = new GraphPresetRepository(directory);
                repository.Write("good", MakeData());

                File.WriteAllText(Path.Combine(directory, "notes.txt"), "not a preset");
                File.WriteAllText(Path.Combine(directory, "broken.json"), "{ this is not json");
                File.WriteAllText(Path.Combine(directory, "old.json"), "{\"version\":99,\"nodes\":[],\"edges\":[]}");
                File.WriteAllText(Path.Combine(directory, "half-written.json.tmp"), "{\"version\":1,\"nodes\":[],\"edges\":[]}");

                Assert.That(repository.GetAll().Select(x => x.Name), Is.EqualTo(new[] { "good" }));
            }
            finally
            {
                if (Directory.Exists(directory)) Directory.Delete(directory, true);
            }
        }

        /// <remarks>
        /// パターン照合ではなく自前で拡張子を見ているのは、Unix 上の照合が大文字小文字を
        /// 区別するため。macOS の既定のファイルシステムは区別しないので、Finder で付いた
        /// .JSON を取りこぼすと「Finderにはあるのに一覧に出ない」になる。
        /// </remarks>
        [Test]
        public void ListPicksUpAnUppercaseExtension()
        {
            var directory = TempDirectory();
            try
            {
                Directory.CreateDirectory(directory);
                File.WriteAllText(Path.Combine(directory, "shouty.JSON"), JsonUtility.ToJson(MakeData()));

                Assert.That(new GraphPresetRepository(directory).GetAll().Select(x => x.Name), Is.EqualTo(new[] { "shouty" }));
            }
            finally
            {
                if (Directory.Exists(directory)) Directory.Delete(directory, true);
            }
        }

        [Test]
        public void DeleteRemovesOnlyTheNamedPreset()
        {
            var directory = TempDirectory();
            try
            {
                var repository = new GraphPresetRepository(directory);
                repository.Write("a", MakeData());
                repository.Write("b", MakeData());

                Assert.That(repository.Delete("a"), Is.True);

                Assert.That(repository.Read("a"), Is.Null);
                Assert.That(repository.TryGetInfo("a", out _), Is.False);
                Assert.That(repository.TryGetInfo("b", out _), Is.True);
            }
            finally
            {
                if (Directory.Exists(directory)) Directory.Delete(directory, true);
            }
        }

        /// <remarks>「消えている状態にする」操作なので、元から無くても成功。あるかは呼ぶ側が見る。</remarks>
        [Test]
        public void DeleteIsIdempotent()
        {
            var directory = TempDirectory();
            try
            {
                var repository = new GraphPresetRepository(directory);

                Assert.That(repository.Delete("a"), Is.True);

                repository.Write("a", MakeData());
                Assert.That(repository.Delete("a"), Is.True);
                Assert.That(repository.Delete("a"), Is.True);
            }
            finally
            {
                if (Directory.Exists(directory)) Directory.Delete(directory, true);
            }
        }

        /// <remarks>
        /// 名前は検証しない。ファイル名にできない名前はファイルシステムが弾き、
        /// その失敗をそのまま false で返す。呼ぶ側はそれだけ見ればよい。
        /// </remarks>
        [Test]
        public void FailsInsteadOfThrowingOnANameThatCannotBeAFile()
        {
            var directory = TempDirectory();
            try
            {
                var repository = new GraphPresetRepository(directory);

                LogAssert.ignoreFailingMessages = true;

                Assert.That(repository.Write("\0", MakeData()), Is.False);
                Assert.That(repository.Read("\0"), Is.Null);
                Assert.That(repository.Exists("\0"), Is.False);
                Assert.That(repository.Delete("\0"), Is.False);
                Assert.That(repository.TryGetInfo("\0", out _), Is.False);
            }
            finally
            {
                LogAssert.ignoreFailingMessages = false;
                if (Directory.Exists(directory)) Directory.Delete(directory, true);
            }
        }

        [Test]
        public void DefaultNameAvoidsCollisions()
        {
            var directory = TempDirectory();
            try
            {
                var repository = new GraphPresetRepository(directory);

                var first = repository.NextDefaultName();
                Assert.That(first, Is.Not.Empty);

                repository.Write(first, MakeData());
                var second = repository.NextDefaultName();

                Assert.That(second, Is.Not.EqualTo(first));
                Assert.That(repository.Exists(second), Is.False);
            }
            finally
            {
                if (Directory.Exists(directory)) Directory.Delete(directory, true);
            }
        }
    }
}
