using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using UnityEngine;

#nullable enable

namespace Rector.UI.Graphs.Serialization
{
    /// <summary>一覧に出すためのプリセットの中身。</summary>
    public readonly struct GraphPresetInfo
    {
        /// <summary>拡張子を除いたファイル名。ディスク上の綴りをそのまま持つ。</summary>
        public readonly string Name;

        public readonly int NodeCount;
        public readonly int EdgeCount;

        /// <summary>保存日時の表示用文字列。</summary>
        public readonly string SavedAt;

        public GraphPresetInfo(string name, int nodeCount, int edgeCount, string savedAt)
        {
            Name = name;
            NodeCount = nodeCount;
            EdgeCount = edgeCount;
            SavedAt = savedAt;
        }
    }

    /// <summary>
    /// グラフの保存ファイルをプリセットとして扱う。ファイル名がそのままプリセットの名前。
    /// </summary>
    /// <remarks>
    /// HUD はゲームパッド操作でテキスト入力の口が無いので、リネームは Finder に任せる。
    /// 名前の正はディスクにあり、アプリが書いたとは限らない。検証はせず、書けない名前は
    /// ファイルシステムが弾いた失敗をそのまま false で返す。
    ///
    /// ファイルは数 KB なので同期 IO で読み書きする。
    /// </remarks>
    public sealed class GraphPresetRepository
    {
        const string Extension = ".json";
        const string DefaultNameFormat = "yyyy-MM-dd HHmm";

        readonly string directory;

        public GraphPresetRepository() : this(Path.Combine(Application.persistentDataPath, "graphs"))
        {
        }

        public GraphPresetRepository(string directory)
        {
            this.directory = directory;
        }

        string PathOf(string name) => Path.Combine(directory, name + Extension);

        /// <summary>今そこにある読めるプリセットを名前順に並べる。</summary>
        /// <remarks>
        /// 拡張子をパターンに任せないのは、Unix 上の照合が大文字小文字を区別する一方
        /// macOS の既定のファイルシステムは区別せず、Finder で付いた .JSON を取りこぼすため。
        /// 読めないファイルはログを出さずに飛ばす。一覧を開くたび通るので、壊れたファイルが
        /// 1つあると HUD のコンソールが毎回埋まる。
        /// </remarks>
        public GraphPresetInfo[] GetAll()
        {
            var infos = new List<GraphPresetInfo>();

            try
            {
                if (!Directory.Exists(directory)) return Array.Empty<GraphPresetInfo>();

                foreach (var path in Directory.EnumerateFiles(directory))
                {
                    if (!path.EndsWith(Extension, StringComparison.OrdinalIgnoreCase)) continue;

                    var data = ReadFile(path, false);
                    if (data == null) continue;

                    infos.Add(new GraphPresetInfo(
                        Path.GetFileNameWithoutExtension(path),
                        data.nodes.Length,
                        data.edges.Length,
                        FormatSavedAt(data.savedAt)));
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to list graph presets in '{directory}': {e.Message}");
            }

            infos.Sort(static (a, b) => string.Compare(a.Name, b.Name, StringComparison.OrdinalIgnoreCase));
            return infos.ToArray();
        }

        /// <summary>ディスク上の綴りごと引く。見つからなければ false。</summary>
        public bool TryGetInfo(string name, out GraphPresetInfo info)
        {
            info = default;

            if (!TryResolve(name, out var path, out var actualName)) return false;

            var data = ReadFile(path, false);
            if (data == null) return false;

            info = new GraphPresetInfo(actualName, data.nodes.Length, data.edges.Length, FormatSavedAt(data.savedAt));
            return true;
        }

        /// <summary>
        /// 名前をディスク上の綴りへ解決する。中身は読まない。
        /// 大文字小文字を区別しないファイルシステムだと、引数の綴りのまま返すと一覧と食い違う。
        /// </summary>
        bool TryResolve(string name, out string path, out string actualName)
        {
            path = "";
            actualName = "";

            try
            {
                if (!Directory.Exists(directory)) return false;

                foreach (var candidate in Directory.EnumerateFiles(directory))
                {
                    if (!candidate.EndsWith(Extension, StringComparison.OrdinalIgnoreCase)) continue;

                    var stem = Path.GetFileNameWithoutExtension(candidate);
                    if (!string.Equals(stem, name, StringComparison.OrdinalIgnoreCase)) continue;

                    path = candidate;
                    actualName = stem;
                    return true;
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to look up graph preset '{name}' in '{directory}': {e.Message}");
            }

            return false;
        }

        /// <summary>読めるかどうかに関わらず、その名前のファイルがあるか。</summary>
        public bool Exists(string name)
        {
            try
            {
                return File.Exists(PathOf(name));
            }
            catch (Exception)
            {
                // 名前がパスとして成立しない。存在しない扱いにして、書き込みの失敗で気付かせる
                return false;
            }
        }

        /// <summary>空いている既定名。保存日時をそのまま名前にし、同じ分に重なったら連番を送る。</summary>
        public string NextDefaultName()
        {
            var now = DateTime.Now;
            var stem = now.ToString(DefaultNameFormat, CultureInfo.InvariantCulture);
            if (!Exists(stem)) return stem;

            for (var i = 2; i < 100; i++)
            {
                var candidate = $"{stem} {i}";
                if (!Exists(candidate)) return candidate;
            }

            return now.ToString("yyyy-MM-dd HHmmss", CultureInfo.InvariantCulture);
        }

        /// <summary>保存フォルダを開く。名前を変える唯一の口。</summary>
        /// <remarks>
        /// "file://" + パス の連結にしないこと。persistentDataPath は "Application Support" の
        /// 下で空白を含み、エンコードしないと URL として成立せず、例外もログも無く何も起きない。
        /// </remarks>
        public void OpenDirectory()
        {
            try
            {
                Directory.CreateDirectory(directory);
                Application.OpenURL(new Uri(directory).AbsoluteUri);
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to open '{directory}': {e.Message}");
            }
        }

        /// <summary>読めなければ null。壊れたファイルや対応していない版は例外にせず null にしてログを残す。</summary>
        public GraphSaveData? Read(string name)
        {
            try
            {
                // PathOf の Path.Combine はファイル名にできない文字で投げる
                return ReadFile(PathOf(name), true);
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to read graph preset '{name}': {e.Message}");
                return null;
            }
        }

        /// <remarks>
        /// 版が違うファイルは半端に読まず必ず断る。JsonUtility は知らないキーを黙って捨て、
        /// 無いキーを初期値のままにするので、版を見ないと「一部だけ拾えた別物のグラフ」ができる。
        /// </remarks>
        static GraphSaveData? ReadFile(string path, bool logErrors)
        {
            try
            {
                if (!File.Exists(path)) return null;

                var data = JsonUtility.FromJson<GraphSaveData>(File.ReadAllText(path));
                // JsonUtility は空文字や "null" で null を返す
                if (data == null) return null;

                if (!data.IsSupportedVersion)
                {
                    if (logErrors)
                    {
                        Debug.LogError($"Graph preset '{Path.GetFileNameWithoutExtension(path)}' has unsupported version {data.version} (expected {GraphSaveData.CurrentVersion}).");
                    }

                    return null;
                }

                return data;
            }
            catch (Exception e)
            {
                if (logErrors)
                {
                    Debug.LogError($"Failed to read graph preset '{Path.GetFileNameWithoutExtension(path)}': {e.Message}");
                }

                return null;
            }
        }

        /// <remarks>
        /// 一時ファイルへ書いてから置き換える。保存中に落ちても、既に入っていたグラフを
        /// 中途半端なファイルで潰さないため。名前は検証せず、ファイル名にできない名前は
        /// ここで例外になり false で返る。
        /// </remarks>
        public bool Write(string name, GraphSaveData data)
        {
            // パスを組む前に落ちることがあるので、catch 側では組み直さない
            string? temp = null;

            try
            {
                var path = PathOf(name);
                temp = path + ".tmp";

                Directory.CreateDirectory(directory);
                File.WriteAllText(temp, JsonUtility.ToJson(data, true));

                // File.Replace は置き換え先が無いと失敗する
                if (File.Exists(path)) File.Replace(temp, path, null);
                else File.Move(temp, path);

                return true;
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to write graph preset '{name}': {e.Message}");
                if (temp != null) TryDelete(temp);
                return false;
            }
        }

        /// <summary>
        /// プリセットのファイルを消す。元から無いものを消すのは成功扱い(冪等)で、
        /// false は「消せなかった」ときだけ。あるかどうかは呼ぶ側が判断する。
        /// </summary>
        public bool Delete(string name)
        {
            try
            {
                var path = PathOf(name);
                if (!File.Exists(path)) return true;

                File.Delete(path);
                return true;
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to delete graph preset '{name}': {e.Message}");
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
