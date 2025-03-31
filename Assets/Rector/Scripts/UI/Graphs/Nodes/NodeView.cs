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

        public Vector2 Position
        {
            get => Root.transform.position;
            set => Root.transform.position = new Vector3(value.x, value.y, 0);
        }

        public NodeView(VisualElement templateContainer, Node node)
        {
            Root = templateContainer.Q<VisualElement>("node");
            NameLabel = Root.Q<Label>("name-label");
            InputSlotList = Root.Q<VisualElement>("input-slot-list");
            OutputSlotList = Root.Q<VisualElement>("output-slot-list");

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
    }
}
