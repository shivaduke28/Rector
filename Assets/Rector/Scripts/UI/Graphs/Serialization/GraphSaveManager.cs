using Rector.UI.GraphPages;

#nullable enable

namespace Rector.UI.Graphs.Serialization
{
    /// <summary>
    /// グラフのスロットへの保存と読み込み。HUD と CLI の共通の入り口。
    /// </summary>
    public sealed class GraphSaveManager
    {
        readonly GraphSerializer serializer;
        readonly GraphSlotRepository repository;

        public GraphSaveManager(GraphPage graphPage, NodeTemplateRepository nodeTemplateRepository)
        {
            serializer = new GraphSerializer(graphPage, nodeTemplateRepository);
            repository = new GraphSlotRepository();
        }

        public GraphSlotInfo[] GetAllSlotInfo() => repository.GetAllInfo();

        public GraphSlotInfo GetSlotInfo(int slot) => repository.GetInfo(slot);

        public bool Save(int slot, out GraphTransferResult result)
        {
            var data = serializer.Capture(out result);
            if (!repository.Write(slot, data)) return false;

            RectorLogger.GraphSaved(slot, result.NodeCount, result.EdgeCount, result.SkippedNodeCount, result.SkippedEdgeCount);
            return true;
        }

        /// <summary>空のスロットや壊れたファイルでは false を返し、グラフには触らない。</summary>
        public bool Load(int slot, out GraphTransferResult result)
        {
            result = default;

            var data = repository.Read(slot);
            if (data == null)
            {
                RectorLogger.GraphSlotEmpty(slot);
                return false;
            }

            result = serializer.Restore(data);
            RectorLogger.GraphLoaded(slot, result.NodeCount, result.EdgeCount, result.SkippedNodeCount, result.SkippedEdgeCount);
            return true;
        }
    }
}
