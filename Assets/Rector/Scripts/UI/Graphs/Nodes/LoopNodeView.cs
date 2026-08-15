using R3;
using UnityEngine.UIElements;

namespace Rector.UI.Graphs.Nodes
{
    public sealed class LoopNodeView : NodeView
    {
        public LoopNodeView(VisualElement templateContainer, LoopNode loopNode) : base(templateContainer, loopNode)
        {
            // 名前側にlength（静的設定）、位置はDAWのbar.beat風に cycle.phase (issue #151)。
            // phase をゼロ埋めして桁数変化でノード幅が揺れないようにする（SequenceNodeView と同じ方針）
            loopNode.Beat.CombineLatest(loopNode.Length, (beat, len) =>
                {
                    var l = len < 1 ? 1 : len;
                    var digits = l.ToString().Length;
                    var cycle = beat >= 1 ? (beat - 1) / l + 1 : 0;
                    var phase = beat >= 1 ? (beat - 1) % l + 1 : 0;
                    return $"Loop{l} {cycle}.{phase.ToString().PadLeft(digits, '0')}";
                })
                .Subscribe(text => NameLabel.text = text)
                .AddTo(Disposables);
        }
    }
}
