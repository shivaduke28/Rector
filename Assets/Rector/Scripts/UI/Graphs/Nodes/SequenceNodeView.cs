using R3;
using UnityEngine;
using UnityEngine.UIElements;

namespace Rector.UI.Graphs.Nodes
{
    public sealed class SequenceNodeView : NodeView
    {
        // カテゴリはSequenceのまま、activeを持つ源としてEventのplayアイコンを使う
        protected override Texture2D GetOutlineIcon(Node node) => VisualElementFactory.Instance.Icons.@event;
        protected override Texture2D GetFilledIcon(Node node) => VisualElementFactory.Instance.Icons.eventFilled;

        public SequenceNodeView(VisualElement templateContainer, SequenceNode sequenceNode) : base(templateContainer, sequenceNode)
        {
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
