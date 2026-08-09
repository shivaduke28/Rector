using System;
using System.Globalization;
using System.IO;
using UnityEngine;

#nullable enable

namespace Rector.UI.Graphs.Serialization
{
    /// <summary>一覧に出すためのスロットの中身。</summary>
    public readonly struct GraphSlotInfo
    {
        /// <summary>1 始まり。HUD の表示と揃える。</summary>
        public readonly int Number;

        public readonly bool IsEmpty;
        public readonly int NodeCount;
        public readonly int EdgeCount;

        /// <summary>保存日時の表示用文字列。空のスロットでは空。</summary>
        public readonly string SavedAt;

        public GraphSlotInfo(int number, bool isEmpty, int nodeCount, int edgeCount, string savedAt)
        {
            Number = number;
            IsEmpty = isEmpty;
            NodeCount = nodeCount;
            EdgeCount = edgeCount;
            SavedAt = savedAt;
        }

        public static GraphSlotInfo Empty(int number) => new(number, true, 0, 0, "");
    }

    /// <summary>
    /// グラフの保存ファイルを固定スロットとして扱う。
    /// </summary>
    /// <remarks>
    /// HUD は全てゲームパッド操作でテキスト入力の口が無いため、名前を付けさせず番号で持つ。
    /// ファイルは数 KB なので同期 IO で読み書きする。
    /// </remarks>
    public sealed class GraphSlotRepository
    {
        public const int SlotCount = 8;

        readonly string directory;

        public GraphSlotRepository() : this(Path.Combine(Application.persistentDataPath, "graphs"))
        {
        }

        public GraphSlotRepository(string directory)
        {
            this.directory = directory;
        }

        public static bool IsValidSlot(int number) => number >= 1 && number <= SlotCount;

        string PathOf(int number) => Path.Combine(directory, $"slot{number}.json");

        public GraphSlotInfo GetInfo(int number)
        {
            if (!IsValidSlot(number)) return GraphSlotInfo.Empty(number);

            var data = Read(number);
            if (data == null) return GraphSlotInfo.Empty(number);

            return new GraphSlotInfo(number, false, data.nodes.Length, data.edges.Length, FormatSavedAt(data.savedAt));
        }

        public GraphSlotInfo[] GetAllInfo()
        {
            var infos = new GraphSlotInfo[SlotCount];
            for (var i = 0; i < SlotCount; i++)
            {
                infos[i] = GetInfo(i + 1);
            }

            return infos;
        }

        /// <summary>
        /// 読めなければ null。壊れたファイルや対応していない版は例外にせず null にしてログを残す。
        /// </summary>
        /// <remarks>
        /// 版が違うファイルは半端に読まず必ず断る。JsonUtility は知らないキーを黙って捨て、
        /// 無いキーを初期値のままにするので、版を見ないと「一部だけ拾えた別物のグラフ」ができる。
        /// </remarks>
        public GraphSaveData? Read(int number)
        {
            if (!IsValidSlot(number)) return null;

            var path = PathOf(number);
            if (!File.Exists(path)) return null;

            try
            {
                var data = JsonUtility.FromJson<GraphSaveData>(File.ReadAllText(path));
                // JsonUtility は空文字や "null" で null を返す
                if (data == null) return null;

                if (!data.IsSupportedVersion)
                {
                    Debug.LogError($"Graph slot {number} has unsupported version {data.version} (expected {GraphSaveData.CurrentVersion}).");
                    return null;
                }

                return data;
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to read graph slot {number}: {e.Message}");
                return null;
            }
        }

        /// <remarks>
        /// 一時ファイルへ書いてから置き換える。保存中に落ちても、既に入っていたグラフを
        /// 中途半端なファイルで潰さないため。
        /// </remarks>
        public bool Write(int number, GraphSaveData data)
        {
            if (!IsValidSlot(number)) return false;

            var path = PathOf(number);
            var temp = path + ".tmp";

            try
            {
                Directory.CreateDirectory(directory);
                File.WriteAllText(temp, JsonUtility.ToJson(data, true));

                // File.Replace は置き換え先が無いと失敗する
                if (File.Exists(path)) File.Replace(temp, path, null);
                else File.Move(temp, path);

                return true;
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to write graph slot {number}: {e.Message}");
                TryDelete(temp);
                return false;
            }
        }

        /// <summary>スロットのファイルを消す。空のスロットを消すのは成功扱い(冪等)。</summary>
        /// <remarks>
        /// false は「番号が範囲外」か「消せなかった」のときだけ。空かどうかは呼ぶ側が
        /// <see cref="GetInfo"/> で判断する。Load と同じで、状態の判定と操作の成否を混ぜない。
        /// </remarks>
        public bool Delete(int number)
        {
            if (!IsValidSlot(number)) return false;

            var path = PathOf(number);
            if (!File.Exists(path)) return true;

            try
            {
                File.Delete(path);
                return true;
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to delete graph slot {number}: {e.Message}");
                return false;
            }
        }

        static void TryDelete(string path)
        {
            try
            {
                if (File.Exists(path)) File.Delete(path);
            }
            catch (Exception e)
            {
                Debug.LogWarning($"Failed to clean up '{path}': {e.Message}");
            }
        }

        /// <summary>ISO 8601 を一覧向けの短い形にする。読めなければそのまま返す。</summary>
        static string FormatSavedAt(string? savedAt)
        {
            if (string.IsNullOrEmpty(savedAt)) return "";

            return DateTime.TryParse(savedAt, CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsed)
                ? parsed.ToString("yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture)
                : savedAt;
        }
    }
}
