namespace Rector
{
    /// <summary>
    /// ノードの分類。ノードエディタの表示だけでなく、シーン上の NodeBehaviour も持つので
    /// UI 側ではなくここに置く。
    /// </summary>
    public enum NodeCategory
    {
        Vfx,
        Camera,
        Event,
        Operator,
        Math,
        Scene,
        System,

        // NodeBehaviour が [SerializeField] で持つので、Unity は整数で直列化する。
        // 並べ替えるとシーン上のノードのカテゴリが化けるため、順序は動かさないこと
        Input
    }
}
