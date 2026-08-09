using Rector.UI.GraphPages;

#nullable enable

namespace Rector.UI.Graphs.Serialization
{
    /// <summary>
    /// グラフのプリセットへの保存と読み込み。HUD と CLI の共通の入り口。
    /// </summary>
    public sealed class GraphSaveManager
    {
        readonly GraphSerializer serializer;
        readonly GraphPresetRepository repository;

        public GraphSaveManager(GraphPage graphPage, NodeTemplateRepository nodeTemplateRepository)
            : this(new GraphSerializer(graphPage, nodeTemplateRepository), new GraphPresetRepository())
        {
        }

        public GraphSaveManager(GraphSerializer serializer, GraphPresetRepository repository)
        {
            this.serializer = serializer;
            this.repository = repository;
        }

        public GraphPresetInfo[] GetAll() => repository.GetAll();

        public bool TryGetInfo(string name, out GraphPresetInfo info) => repository.TryGetInfo(name, out info);

        public bool Exists(string name) => repository.Exists(name);

        public string NextDefaultName() => repository.NextDefaultName();

        /// <summary>保存フォルダを開く。プリセットの名前を変える唯一の口。</summary>
        public void OpenDirectory() => repository.OpenDirectory();

        public bool Save(string name, out GraphSaveResult result)
        {
            var data = serializer.Capture(out result);
            if (!repository.Write(name, data)) return false;

            RectorLogger.GraphSaved(name, result.NodeCount, result.EdgeCount, result.SkippedNodeCount, result.SkippedEdgeCount);
            return true;
        }

        /// <summary>プリセットを消す。元から無くても成功扱い。グラフには触らない。</summary>
        public bool Delete(string name)
        {
            if (!repository.Delete(name)) return false;

            RectorLogger.GraphPresetDeleted(name);
            return true;
        }

        /// <summary>無いプリセットや壊れたファイルでは false を返し、グラフには触らない。</summary>
        public bool Load(string name, out GraphLoadResult result)
        {
            result = default;

            var data = repository.Read(name);
            if (data == null)
            {
                RectorLogger.GraphPresetMissing(name);
                return false;
            }

            result = serializer.Restore(data);
            RectorLogger.GraphLoaded(name, result.NodeCount, result.EdgeCount, result.SkippedNodeCount, result.SkippedEdgeCount);
            return true;
        }
    }
}
