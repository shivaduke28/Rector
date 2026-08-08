using System;
using UnityEngine;

namespace Rector.UI.Graphs.Serialization
{
    /// <summary>
    /// 保存されたグラフ。JsonUtility で読み書きするので、多相も Dictionary も使わず
    /// 値の型ごとに配列を分けた平たい形にしている。
    /// </summary>
    /// <remarks>
    /// これは永続化データの形なので、後方互換を壊す変更をするなら version を上げること。
    /// スロットは index で指す。ノードのスロット構成を変えると古いファイルの index はズレるが、
    /// エッジには型を持たせてあるので、ズレたエッジは別のスロットに繋がらず落ちる。
    /// ノードの識別子は nodes 配列の index。NodeId は起動ごとの採番なので保存しない。
    /// </remarks>
    [Serializable]
    public sealed class GraphSaveData
    {
        public const int CurrentVersion = 1;

        /// <summary>
        /// 形式の版数。既定値を持たせないのは、version の無い JSON を v1 と誤認しないため。
        /// </summary>
        public int version;

        /// <summary>保存日時。一覧に出すだけで、復元には使わない。</summary>
        public string savedAt = "";

        public NodeSaveData[] nodes = Array.Empty<NodeSaveData>();
        public EdgeSaveData[] edges = Array.Empty<EdgeSaveData>();

        public bool IsSupportedVersion => version == CurrentVersion;
    }

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

    /// <remarks>
    /// 配列の初期化子は必須。JsonUtility は JSON にキーが無いフィールドを初期値のままにするので、
    /// 初期化子を消すと欠けたキーが null になり、復元側の foreach が落ちる。
    /// </remarks>
    [Serializable]
    public sealed class NodeSaveData
    {
        // --- NodeTemplateId。直和なので、templateKind が示すフィールドだけが埋まる ---

        /// <summary>NodeTemplateKind の名前。"Code" または "Behaviour"。</summary>
        public string templateKind = "";

        /// <summary>templateKind == Code のとき、ノードクラスの Type.Name。</summary>
        public string nodeType = "";

        /// <summary>templateKind == Behaviour のとき、NodeBehaviour.guid。</summary>
        public string behaviourGuid = "";

        // 値の型はどの配列に入っているかで決まる。復元時は同じ型の入力スロットにしか入らない。
        public FloatSlotValue[] floats = Array.Empty<FloatSlotValue>();
        public IntSlotValue[] ints = Array.Empty<IntSlotValue>();
        public BoolSlotValue[] bools = Array.Empty<BoolSlotValue>();
        public Vector3SlotValue[] vector3s = Array.Empty<Vector3SlotValue>();
    }

    [Serializable]
    public sealed class FloatSlotValue
    {
        public int index;
        public float value;
    }

    [Serializable]
    public sealed class IntSlotValue
    {
        public int index;
        public int value;
    }

    [Serializable]
    public sealed class BoolSlotValue
    {
        public int index;
        public bool value;
    }

    [Serializable]
    public sealed class Vector3SlotValue
    {
        public int index;
        public Vector3 value;
    }

    /// <remarks>
    /// fromType / toType は SlotValueType の名前。復元時に今のスロットの型と突き合わせ、
    /// 食い違えばそのエッジを落とす。これが無いと、スロットがズレたときに
    /// 型だけ合う別のスロットへ黙って繋がってしまう。
    /// </remarks>
    [Serializable]
    public sealed class EdgeSaveData
    {
        /// <summary>GraphSaveData.nodes の index。</summary>
        public int fromNode;

        public int fromSlot;
        public string fromType = "";

        public int toNode;
        public int toSlot;
        public string toType = "";
    }
}
