using R3;
using Rector.UI.LayeredGraphDrawing;

namespace Rector.UI.GraphPages.NodeParameters
{
    /// <summary>
    /// ノードが属するカラム。入力スロットではないが、どのノードにもあるので
    /// パラメータの先頭に置いて他のパラメータと同じ十字キー操作で動かせるようにする。
    /// </summary>
    public sealed class ExposedColumnInputModel : IExposedInputModel
    {
        public const string Name = "Column";

        public readonly ReactiveProperty<bool> IsFocused = new(false);

        /// <summary>ヘッダーの "COLUMN 1" に合わせて1始まりで見せる。</summary>
        public readonly ReactiveProperty<int> Value;

        public int MinValue => 1;
        public int MaxValue { get; }

        public ExposedColumnInputModel(GraphPage page, LayeredNode node)
        {
            MaxValue = page.Columns.CurrentCount;
            Value = new ReactiveProperty<int>(node.Column + 1);

            // スライダーを直接動かされた場合もここを通る。
            // MoveNodeToColumnは値が変わらなければ何もしないのでループしない。
            Value.Subscribe(x => page.MoveNodeToColumn(node, x - 1));
        }

        public void Increment() => Value.Value = Wrap(Value.Value + 1);
        public void Decrement() => Value.Value = Wrap(Value.Value - 1);

        int Wrap(int oneBased) => (oneBased - 1 + MaxValue) % MaxValue + 1;

        public void Focus() => IsFocused.Value = true;
        public void Unfocus() => IsFocused.Value = false;
    }
}
