using R3;
using UnityEngine;
using UnityEngine.UIElements;

namespace Rector.UI.Graphs.Nodes
{
    public sealed class ChanceNodeView : NodeView
    {
        public ChanceNodeView(VisualElement templateContainer, ChanceNode chanceNode) : base(templateContainer, chanceNode)
        {
            // 確率を「Chance 50%」の形で名前ラベルに出す (issue #162)
            chanceNode.Chance
                .Select(c => $"Chance {Mathf.RoundToInt(Mathf.Clamp01(c) * 100f)}%")
                .Subscribe(text => NameLabel.text = text)
                .AddTo(Disposables);
        }
    }
}
