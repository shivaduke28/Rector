namespace Rector.UI.GraphPages.NodeParameters
{
    /// <summary>
    /// カーソルが止まって操作できる行。
    /// </summary>
    /// <remarks>
    /// 操作をここに載せているのは、種類を増やすたびに NodeParameterModel の型switchを
    /// 何箇所も直す羽目になり、書き忘れてもコンパイルが通ってしまうため。
    /// 操作を持たない行は <see cref="IExposedRow"/> だけを実装する。
    /// </remarks>
    public interface IExposedInputModel : IExposedRow
    {
        void Focus();
        void Unfocus();

        /// <summary>十字キー右。</summary>
        void Increment();

        /// <summary>十字キー左。</summary>
        void Decrement();

        /// <summary>Actionボタン。</summary>
        void DoAction();
    }
}
