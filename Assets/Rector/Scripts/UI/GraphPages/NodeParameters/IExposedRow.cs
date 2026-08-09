namespace Rector.UI.GraphPages.NodeParameters
{
    /// <summary>
    /// ノードパラメータパネルに並ぶ1行。
    /// </summary>
    /// <remarks>
    /// 1つの入力スロットが必ず1行になるとは限らない。Vector3は見出しと3成分の4行になり、
    /// そのうち見出しは操作を持たない。行と「操作できる行」(<see cref="IExposedInputModel"/>)を
    /// 型で分けてあるので、カーソルが止まる行の一覧はこの一覧から機械的に導ける。
    /// </remarks>
    public interface IExposedRow
    {
        string Label { get; }
    }
}
