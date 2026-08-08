using System;

namespace Rector.UI.Graphs.Serialization
{
    /// <summary>NodeTemplateId と保存用フィールドの相互変換。</summary>
    public static class NodeTemplateIdSaveData
    {
        public static void Write(NodeTemplateId id, NodeSaveData data)
        {
            data.templateKind = id.Kind.ToString();
            data.nodeType = id.Kind == NodeTemplateKind.Code ? id.TypeName : "";
            data.behaviourGuid = id.Kind == NodeTemplateKind.Behaviour ? id.Guid.ToString() : "";
        }

        /// <summary>読めなければ IsValid が false の値を返す。呼び出し側はそのノードを捨てる。</summary>
        public static NodeTemplateId Read(NodeSaveData data)
        {
            if (!Enum.TryParse<NodeTemplateKind>(data.templateKind, out var kind)) return default;

            switch (kind)
            {
                case NodeTemplateKind.Code:
                    return string.IsNullOrEmpty(data.nodeType) ? default : NodeTemplateId.Code(data.nodeType);
                case NodeTemplateKind.Behaviour:
                    return Guid.TryParse(data.behaviourGuid, out var guid) ? NodeTemplateId.Behaviour(guid) : default;
                default:
                    return default;
            }
        }
    }
}
