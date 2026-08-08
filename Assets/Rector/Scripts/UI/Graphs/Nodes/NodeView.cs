using System;
using System.Collections.Generic;
using R3;
using Rector.UI.Graphs.Slots;
using UnityEngine;
using UnityEngine.UIElements;

namespace Rector.UI.Graphs.Nodes
{
    public class NodeView : IDisposable
    {
        protected readonly VisualElement Root;
        protected readonly Label NameLabel;
        protected readonly VisualElement InputSlotList;
        protected readonly VisualElement OutputSlotList;

        public Node Node { get; }
        public float Width => Root.resolvedStyle.width;
        public float Height => Root.resolvedStyle.height;
        protected const string NodeSelectedClassName = "rector-node--selected";
        public List<InputSlotView> InputSlotViews { get; }
        public List<OutputSlotView> OutputSlotViews { get; }

        protected readonly CompositeDisposable Disposables = new();

        // アニメーション中は補間中の現在値を返す。目標値が要る場合は TargetPosition を使う。
        public Vector2 Position
        {
            get => Root.resolvedStyle.translate;
            set
            {
                TargetPosition = value;
                Root.style.translate = value;
            }
        }

        public Vector2 TargetPosition { get; private set; }

        public NodeView(VisualElement templateContainer, Node node)
        {
            Root = templateContainer.Q<VisualElement>("node");
            NameLabel = Root.Q<Label>("name-label");
            InputSlotList = Root.Q<VisualElement>("input-slot-list");
            OutputSlotList = Root.Q<VisualElement>("output-slot-list");

            Root.Q<VisualElement>("icon").style.backgroundImage = new StyleBackground(GetCategoryIcon(node.Category));

            Node = node;
            NameLabel.text = node.Name;
            node.Selected.Subscribe(x => Root.EnableInClassList(NodeSelectedClassName, x)).AddTo(Disposables);
            InputSlotViews = new List<InputSlotView>(node.InputSlots.Length);
            foreach (var slot in node.InputSlots)
            {
                var slotView = new InputSlotView(VisualElementFactory.Instance.CreateInputSlot());
                slotView.Bind(slot).AddTo(Disposables);
                slotView.AddTo(InputSlotList);
                InputSlotViews.Add(slotView);
            }

            OutputSlotViews = new List<OutputSlotView>(node.OutputSlots.Length);
            foreach (var slot in node.OutputSlots)
            {
                var slotView = new OutputSlotView(VisualElementFactory.Instance.CreateOutputSlot());
                slotView.Bind(slot).AddTo(Disposables);
                slotView.AddTo(OutputSlotList);
                OutputSlotViews.Add(slotView);
            }

            node.IsMuted.Subscribe(x => Root.EnableInClassList("rector-node--muted", x)).AddTo(Disposables);
        }

        public void Dispose() => Disposables.Dispose();
        public void AddTo(VisualElement parent) => parent.Add(Root);
        public void RemoveFrom(VisualElement parent) => parent.Remove(Root);

        static Texture2D GetCategoryIcon(NodeCategory category)
        {
            return category switch
            {
                NodeCategory.Vfx => VisualElementFactory.Instance.Icons.vfx,
                NodeCategory.Camera => VisualElementFactory.Instance.Icons.camera,
                NodeCategory.Event => VisualElementFactory.Instance.Icons.@event,
                NodeCategory.Operator => VisualElementFactory.Instance.Icons.@operator,
                NodeCategory.Math => VisualElementFactory.Instance.Icons.math,
                NodeCategory.Scene => VisualElementFactory.Instance.Icons.scene,
                NodeCategory.System => VisualElementFactory.Instance.Icons.system,
                NodeCategory.MIDI => VisualElementFactory.Instance.Icons.midi,
                _ => null
            };
        }
    }
}
