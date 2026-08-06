using Rector.UI.GraphPages.NodeParameters;
using UnityEngine;

namespace Rector.UI.GraphPages
{
    public sealed class NodeParameterInputHandler : GraphPageInputHandler
    {
        readonly GraphPage graphPage;
        readonly NodeParameterView view;

        public NodeParameterInputHandler(GraphPage graphPage, NodeParameterView view)
        {
            this.graphPage = graphPage;
            this.view = view;
        }

        public override void Navigate(Vector2 value) => view.Navigate(value);

        // パラメータを開いている間だけノード自体をカラム間で動かせる。
        // 開いていないときの左スティックはフォーカスのカラム移動なので衝突しない。
        public override void MoveColumn(int direction) => graphPage.MoveSelectedNodeToColumn(direction);
        public override void Action() => view.Action();
        public override void CloseNodeParameter() => view.CloseNodeParameter();
        public override void Cancel() => view.CloseNodeParameter();
    }
}
