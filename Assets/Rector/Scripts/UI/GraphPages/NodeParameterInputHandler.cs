using Rector.UI.GraphPages.NodeParameters;
using UnityEngine;

namespace Rector.UI.GraphPages
{
    public sealed class NodeParameterInputHandler : GraphPageInputHandler
    {
        readonly NodeParameterView view;
        public NodeParameterInputHandler(NodeParameterView view)
        {
            this.view = view;
        }

        public override void Navigate(Vector2 value) => view.Navigate(value);
        public override void Action() => view.Action();
        public override void CloseNodeParameter() => view.CloseNodeParameter();
        public override void Cancel() => view.CloseNodeParameter();
    }
}
