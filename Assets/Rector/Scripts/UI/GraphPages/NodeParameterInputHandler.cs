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

        // パネルを開いたままL1を重ねたミュート(MuteChord)がここに届く
        public override void Mute() => graphPage.ToggleMute(graphPage.SelectedNode);
    }
}
