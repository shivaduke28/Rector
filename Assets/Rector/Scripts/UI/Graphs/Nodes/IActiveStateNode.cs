using R3;

namespace Rector.UI.Graphs.Nodes
{
    // アクティブ状態をアイコンの outline⇔filled で表示するノードが実装する。
    // filled テクスチャが未割当のカテゴリでは outline のまま表示される。
    public interface IActiveStateNode
    {
        ReadOnlyReactiveProperty<bool> ActiveState { get; }
    }
}
