using R3;

namespace Rector.UI.GraphPages
{
    /// <summary>
    /// グラフの見え方の設定。ノードやグループの構造とは関係しないものを置く。
    /// </summary>
    public sealed class GraphViewSettings
    {
        /// <summary>
        /// 選択中のノードに合わせてグラフを動かすか。
        /// </summary>
        /// <remarks>
        /// 今の実装は選択のたびにノードを画面中央へ持ってくる。手で見たい位置に置いたまま
        /// ノードを辿りたいときは邪魔になるので切れるようにしている。
        /// </remarks>
        public ReactiveProperty<bool> FollowSelectedNode { get; } = new(true);
    }
}
