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

        readonly GraphPage page;
        readonly LayeredNode node;

        public ExposedGroupInputModel(GraphPage page, LayeredNode node)
        {
            this.page = page;
            this.node = node;
            MaxValue = page.Groups.CurrentCount;
            Value = new ReactiveProperty<int>(Current);

            // スライダーを直接動かされた場合もここを通る。
            // MoveNodeToGroupは値が変わらなければ何もしないのでループしない。
            Value.Subscribe(x => page.MoveNodeToGroup(node, x - 1));
        }

        /// <summary>ノードの今のグループ。1始まり。</summary>
        int Current => page.Groups.Fold(node.Group) + 1;

        // パネルを開いている間にCLIなどからグループを変えられていることがあるので、
        // 自分が持っている値ではなくノードの現在値から計算する。
        // 自前の値を起点にすると、外からの変更を巻き戻して別のグループへ飛ばしてしまう。
        public void Increment() => Value.Value = Wrap(Current + 1);
        public void Decrement() => Value.Value = Wrap(Current - 1);

        public void DoAction() { }

        int Wrap(int oneBased) => (oneBased - 1 + MaxValue) % MaxValue + 1;

        public void Focus() => IsFocused.Value = true;
        public void Unfocus() => IsFocused.Value = false;
    }
}
