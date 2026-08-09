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
    /// HUD は全てゲームパッド操作でテキスト入力の口が無い。かつては名前を諦めて slot1..8 の
    /// 固定枠で持っていたが、名前を付ける UI を作る代わりに Finder に投げることにした。
    /// アプリのビルドは windowed で runInBackground なので、保存フォルダを開いて OS の上で
    /// リネームできる。名前の編集 UI を持たずに名前が持てる。
    ///
    /// その代わり名前の正はディスクにあり、アプリが決めた形とは限らない。
    /// 名前の検証はしない。書けない名前はファイルシステムが弾き、その失敗をそのまま false で返す。
    /// 一覧は「今そこにある .json」であって、アプリが書いたものとは限らない。
    ///
    /// ファイルは数 KB なので同期 IO で読み書きする。
    /// </remarks>
    public sealed class GraphPresetRepository
    {
        const string Extension = ".json";

        /// <summary>表示に使う既定名の形。分までなのは、秒まで出すと一覧で読みにくいため。</summary>
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

        /// <summary>
        /// 今そこにある読めるプリセットを名前順に並べる。
        /// </summary>
        /// <remarks>
        /// 拡張子の判定を <see cref="Directory.EnumerateFiles(string,string)"/> のパターンに任せず
        /// 自分で見るのは、Unix 上のパターン照合が大文字小文字を区別するため。macOS の既定の
        /// ファイルシステムは区別しないので、Finder で付けた .JSON が取りこぼされる。
        /// 書き込み中の .json.tmp はこの判定から自然に外れる。
        ///
        /// 読めないファイルはログを出さずに黙って飛ばす。ここは一覧を開くたびに通るので、
        /// 壊れたファイルが1つあると HUD のコンソールが毎回それで埋まる。
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

        /// <summary>
        /// 名前を一覧に照合して、ディスク上の綴りごと引く。見つからなければ false。
        /// </summary>
        /// <remarks>
        /// <see cref="File.Exists"/> で直に見ないのは、macOS の既定のファイルシステムが
        /// 大文字小文字を区別しないため。"Ambient" で引いた結果を "ambient.json" に書き戻すと、
        /// 応答とディスクで綴りが食い違う。一覧を正として実際の名前を返す。
        /// </remarks>
        public bool TryGetInfo(string name, out GraphPresetInfo info)
        {
            foreach (var candidate in GetAll())
            {
                if (!string.Equals(candidate.Name, name, StringComparison.OrdinalIgnoreCase)) continue;

                info = candidate;
                return true;
            }

            info = default;
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

        /// <summary>
        /// 空いている既定名。保存日時をそのまま名前にする。
        /// </summary>
        /// <remarks>
        /// 名前を入力する口が無いので、保存はまずこの名前で作られ、ユーザーが Finder で
        /// 好きな名前に変える。同じ分に2回保存したときのために連番を送る。
        /// </remarks>
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

            // 同じ分に100個は現実的でないが、名前を返さずに済ませるわけにもいかないので秒まで出す
            return now.ToString("yyyy-MM-dd HHmmss", CultureInfo.InvariantCulture);
        }

        /// <summary>保存フォルダをOSのファイルブラウザで開く。名前を変える唯一の口。</summary>
        /// <remarks>
        /// "file://" + パス の文字列連結にしないこと。macOS の persistentDataPath は
        /// "~/Library/Application Support/..." で空白を含み、エンコードしないまま渡すと
        /// URL として成立せず、例外もログも出さずに何も起きない。
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

        /// <summary>
        /// 読めなければ null。壊れたファイルや対応していない版は例外にせず null にしてログを残す。
        /// </summary>
        public GraphSaveData? Read(string name)
        {
            try
            {
                return ReadFile(PathOf(name), true);
            }
            catch (Exception e)
            {
                // PathOf の Path.Combine はファイル名にできない文字で投げる
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
        /// 中途半端なファイルで潰さないため。
        ///
        /// 名前は検証しない。ファイル名にできない名前はここで例外になり false で返る。
        /// </remarks>
        public bool Write(string name, GraphSaveData data)
        {
            // 後始末に使う。パスを組む前に落ちることがあるので、catch 側では組み直さない
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

        /// <summary>プリセットのファイルを消す。元から無いものを消すのは成功扱い(冪等)。</summary>
        /// <remarks>
        /// false は「消せなかった」ときだけ。あるかどうかは呼ぶ側が <see cref="TryGetInfo"/> で
        /// 判断する。Load と同じで、状態の判定と操作の成否を混ぜない。
        /// </remarks>
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
