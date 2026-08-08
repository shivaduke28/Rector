using System;

#nullable enable

namespace Rector.UI.Graphs
{
    public enum NodeTemplateKind
    {
        /// <summary>未設定。default(NodeTemplateId) を弾くために 0 を潰してある。</summary>
        None = 0,

        /// <summary>コードで定義されたノード。ノードのクラス1つにつきテンプレートが1つ。</summary>
        Code,

        /// <summary>NodeBehaviour が裏にいるノード。クラスは共通で、実体ごとにテンプレートがある。</summary>
        Behaviour,
    }

    /// <summary>
    /// ノードテンプレートの識別子。テンプレートの作られ方が2種類しかないので、その直和で表す。
    /// </summary>
    /// <remarks>
    /// 起動を跨いで同じ値になるので、そのままグラフの保存ファイルに書ける。
    ///
    /// Code はノードのクラス名。VFX 名や GameObject 名のような表示用の文字列と違い、
    /// 変えるにはコードを触ることになるので、うっかり保存ファイルを壊しにくい。
    /// Behaviour は NodeBehaviour.guid。prefab やシーンのオブジェクトをリネームしても動かない。
    /// </remarks>
    public readonly struct NodeTemplateId : IEquatable<NodeTemplateId>
    {
        public readonly NodeTemplateKind Kind;

        /// <summary>Kind == Code のときだけ意味を持つ。ノードクラスの Type.Name。</summary>
        public readonly string TypeName;

        /// <summary>Kind == Behaviour のときだけ意味を持つ。</summary>
        public readonly Guid Guid;

        public bool IsValid => Kind != NodeTemplateKind.None;

        NodeTemplateId(NodeTemplateKind kind, string typeName, Guid guid)
        {
            Kind = kind;
            TypeName = typeName;
            Guid = guid;
        }

        /// <remarks>
        /// 型引数で受けるのは、クラスを消したり名前を変えたりしたときにコンパイルで気付くため。
        /// 文字列で書くとビルドが通ってしまい、保存ファイルとの食い違いが実行時まで出てこない。
        /// </remarks>
        public static NodeTemplateId Code<T>() where T : Nodes.Node =>
            new(NodeTemplateKind.Code, typeof(T).Name, Guid.Empty);

        public static NodeTemplateId Code(string typeName) =>
            new(NodeTemplateKind.Code, typeName, Guid.Empty);

        public static NodeTemplateId Behaviour(Guid guid) =>
            new(NodeTemplateKind.Behaviour, "", guid);

        public bool Equals(NodeTemplateId other) =>
            Kind == other.Kind && TypeName == other.TypeName && Guid.Equals(other.Guid);

        public override bool Equals(object? obj) => obj is NodeTemplateId other && Equals(other);

        public override int GetHashCode() => HashCode.Combine((int)Kind, TypeName, Guid);

        public override string ToString() => Kind switch
        {
            NodeTemplateKind.Code => $"code:{TypeName}",
            NodeTemplateKind.Behaviour => $"behaviour:{Guid}",
            _ => "none",
        };

        public static bool operator ==(NodeTemplateId left, NodeTemplateId right) => left.Equals(right);

        public static bool operator !=(NodeTemplateId left, NodeTemplateId right) => !left.Equals(right);
    }
}
