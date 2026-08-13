using R3;
using UnityEngine.UIElements;

namespace Rector.UI.Graphs.Nodes
{
    public sealed class LoopNodeView : NodeView
    {
        public LoopNodeView(VisualElement templateContainer, LoopNode loopNode) : base(templateContainer, loopNode)
        {
            // phase をゼロ埋めして桁数変化でノード幅が揺れないようにする（SequenceNodeView と同じ方針）
            loopNode.Beat.CombineLatest(loopNode.Length, (beat, len) =>
                {
                    var l = len < 1 ? 1 : len;
                    var digits = (l - 1).ToString().Length;
                    return $"Loop {(beat % l).ToString().PadLeft(digits, '0')}/{l}";
                })
                .Subscribe(text => NameLabel.text = text)
                .AddTo(Disposables);
        }
    }
}
