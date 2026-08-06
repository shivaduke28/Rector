using R3;
using Rector.UI.LayeredGraphDrawing;

namespace Rector.UI.GraphPages.NodeParameters
{
    /// <summary>
    /// ノードが属するグループ。入力スロットではないが、どのノードにもあるので
    /// パラメータの先頭に置いて他のパラメータと同じ十字キー操作で動かせるようにする。
    /// </summary>
    public sealed class ExposedGroupInputModel : IExposedInputModel
    {
        public const string Name = "Group";

        public readonly ReactiveProperty<bool> IsFocused = new(false);

        /// <summary>ヘッダーの "GROUP 1" に合わせて1始まりで見せる。</summary>
        public readonly ReactiveProperty<int> Value;

        public int MinValue => 1;
        public int MaxValue { get; }

        public ExposedGroupInputModel(GraphPage page, LayeredNode node)
        {
            MaxValue = page.Groups.CurrentCount;
            Value = new ReactiveProperty<int>(node.Group + 1);

            // スライダーを直接動かされた場合もここを通る。
            // MoveNodeToGroupは値が変わらなければ何もしないのでループしない。
            Value.Subscribe(x => page.MoveNodeToGroup(node, x - 1));
        }

        public void Increment() => Value.Value = Wrap(Value.Value + 1);
        public void Decrement() => Value.Value = Wrap(Value.Value - 1);

        int Wrap(int oneBased) => (oneBased - 1 + MaxValue) % MaxValue + 1;

        public void Focus() => IsFocused.Value = true;
        public void Unfocus() => IsFocused.Value = false;
    }
}
