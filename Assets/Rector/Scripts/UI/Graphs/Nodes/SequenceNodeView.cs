using R3;
using UnityEngine.UIElements;

namespace Rector.UI.Graphs.Nodes
{
    public sealed class SequenceNodeView : NodeView
    {
        public SequenceNodeView(VisualElement templateContainer, SequenceNode sequenceNode) : base(templateContainer, sequenceNode)
        {
            // beat を length の最大値 (len-1) の桁数でゼロ埋めして、桁数変化でノード幅が揺れないようにする
            sequenceNode.Beat.CombineLatest(sequenceNode.Length, (beat, len) =>
                {
                    var digits = (len - 1).ToString().Length;
                    return $"Seq {beat.ToString().PadLeft(digits, '0')}/{len}";
                })
                .Subscribe(text => NameLabel.text = text)
                .AddTo(Disposables);
        }
    }
}
