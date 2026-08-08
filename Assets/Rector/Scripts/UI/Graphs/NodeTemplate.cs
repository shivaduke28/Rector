using System;
using Rector.UI.Graphs.Nodes;

namespace Rector.UI.Graphs
{
    public sealed class NodeTemplate
    {
        public readonly NodeTemplateId Id;
        public readonly NodeCategory Category;
        public readonly string Name;

        /// <summary>
        /// このテンプレートから作ったノードをグラフの保存に含めてよいか。
        /// </summary>
        /// <remarks>
        /// Id とは別の概念。BGシーン由来のテンプレートも Id は持っているが、
        /// シーンがアンロードされると登録ごと消えるので、いまは保存の対象外にしている。
        /// </remarks>
        public readonly bool IsSaveable;

        readonly Func<NodeId, NodeView> factory;

        public NodeView Create(NodeId id)
        {
            var view = factory(id);
            // 生成経路はここ1本なので、どのノードも出自を持って生まれる
            view.Node.TemplateId = Id;
            view.Node.IsSaveable = IsSaveable;
            return view;
        }

        NodeTemplate(NodeTemplateId id, NodeCategory category, string name, bool isSaveable, Func<NodeId, NodeView> factory)
        {
            Id = id;
            Category = category;
            Name = name;
            IsSaveable = isSaveable;
            this.factory = factory;
        }

        /// <summary>コードで定義されたノードのテンプレート。ノードのクラス1つにつき1つ。</summary>
        public static NodeTemplate Code<T>(NodeCategory category, string name, Func<NodeId, NodeView> factory)
            where T : Node =>
            new(NodeTemplateId.Code<T>(), category, name, true, factory);

        /// <summary>NodeBehaviour が裏にいるノードのテンプレート。VFX とカメラ。</summary>
        public static NodeTemplate Behaviour(Guid guid, NodeCategory category, string name, Func<NodeId, NodeView> factory) =>
            new(NodeTemplateId.Behaviour(guid), category, name, true, factory);

        /// <summary>
        /// BGシーンがロードされている間だけ存在するテンプレート。保存の対象外になる。
        /// </summary>
        /// <remarks>
        /// Id は Behaviour と同じ形で持てるが、シーンをアンロードすると登録が消えるため、
        /// 別のシーンでグラフを復元するとスロット構成が分からずエッジも張れない。
        /// 復元の設計は issue #81 から切り出した別 issue で扱う。
        /// </remarks>
        public static NodeTemplate SceneLocal(Guid guid, NodeCategory category, string name, Func<NodeId, NodeView> factory) =>
            new(NodeTemplateId.Behaviour(guid), category, name, false, factory);
    }
}
