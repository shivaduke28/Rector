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
        public override void Action() => view.Action();
        public override void CloseNodeParameter() => view.CloseNodeParameter();
        public override void Cancel() => view.CloseNodeParameter();

        // パネル表示中のミュート(L1/V)がここに届く
        public override void Mute() => graphPage.ToggleMute(graphPage.SelectedNode);

        // R1(SHIFT)を握ったままの△(C)。ノード選択中と同じボタンだが、
        // 向こうが「作成メニューを開く」なのに対しこちらは「同じ種類をもう1個足す」
        public override void AddNode() => graphPage.CopySelectedNode();
    }
}
