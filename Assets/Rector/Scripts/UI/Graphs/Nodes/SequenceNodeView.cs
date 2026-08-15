using R3;
using UnityEngine.UIElements;

namespace Rector.UI.Graphs.Nodes
{
    public sealed class SequenceNodeView : NodeView
    {
        public SequenceNodeView(VisualElement templateContainer, SequenceNode sequenceNode) : base(templateContainer, sequenceNode)
        {
            // カテゴリはSequenceのまま、activeを持つ源としてEventのplayアイコンで表示する
            var icons = VisualElementFactory.Instance.Icons;
            sequenceNode.ActiveState
                .Subscribe(active => Icon.style.backgroundImage = new StyleBackground(active ? icons.eventFilled : icons.@event))
                .AddTo(Disposables);

            // beat を length の桁数でゼロ埋めして、桁数変化でノード幅が揺れないようにする
            sequenceNode.Beat.CombineLatest(sequenceNode.Length, (beat, len) =>
                {
                    var digits = len.ToString().Length;
                    return $"Seq {beat.ToString().PadLeft(digits, '0')}/{len}";
                })
                .Subscribe(text => NameLabel.text = text)
                .AddTo(Disposables);
        }
    }
}
