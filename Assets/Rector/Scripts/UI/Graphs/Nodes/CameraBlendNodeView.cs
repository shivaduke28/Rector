using R3;
using UnityEngine.UIElements;

namespace Rector.UI.Graphs.Nodes
{
    public sealed class CameraBlendNodeView : NodeView
    {
        public CameraBlendNodeView(VisualElement templateContainer, CameraBlendNode cameraBlendNode) : base(templateContainer, cameraBlendNode)
        {
            cameraBlendNode.BlendStyle.Subscribe(style => NameLabel.text = $"Blend {style}").AddTo(Disposables);
        }
    }
}
