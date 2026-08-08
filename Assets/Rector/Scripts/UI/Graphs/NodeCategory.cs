namespace Rector.UI.Graphs
{
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
