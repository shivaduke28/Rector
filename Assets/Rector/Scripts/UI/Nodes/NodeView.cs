using System;
using System.Collections.Generic;
using R3;
using UnityEngine;
using UnityEngine.UIElements;

namespace Rector.UI.Nodes
{
    public class NodeView : IDisposable
    {
        protected readonly VisualElement Root;
        protected readonly Label NameLabel;
        protected readonly VisualElement InputSlotList;
        protected readonly VisualElement OutputSlotList;

        public Node Node { get; protected set; }
        public int LayerIndex;
        public int IndexInLayer;
        public Rect WorldBound => Root.worldBound;
        public float Width => Root.resolvedStyle.width;
        protected const string NodeSelectedClassName = "rector-node--selected";
        public List<InputSlotView> InputSlotViews { get; } = new();
        public List<OutputSlotView> OutputSlotViews { get; } = new();

        protected readonly CompositeDisposable Disposables = new();

        public Vector2 Position
        {
            get => Root.transform.position;
            set => Root.transform.position = new Vector3(value.x, value.y, 0);
        }

        public NodeView(VisualElement templateContainer)
        {
            Root = templateContainer.Q<VisualElement>("node");
            NameLabel = Root.Q<Label>("name-label");
            InputSlotList = Root.Q<VisualElement>("input-slot-list");
            OutputSlotList = Root.Q<VisualElement>("output-slot-list");
        }

        public virtual void Bind(Node node)
        {
            Node = node;
            NameLabel.text = node.Name;
            node.Selected.Subscribe(x => Root.EnableInClassList(NodeSelectedClassName, x)).AddTo(Disposables);
            foreach (var slot in node.InputSlots)
            {
                var slotView = new InputSlotView(VisualElementFactory.Instance.CreateInputSlot());
                slotView.Bind(slot).AddTo(Disposables);
                slotView.AddTo(InputSlotList);
                InputSlotViews.Add(slotView);
            }

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
    }
}
