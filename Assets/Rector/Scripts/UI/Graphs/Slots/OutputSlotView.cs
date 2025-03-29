using System;
using R3;
using UnityEngine;
using UnityEngine.UIElements;

namespace Rector.UI.Graphs.Slots
{
    public sealed class OutputSlotView
    {
        readonly VisualElement root;
        readonly Label nameLabel;
        const string SelectedClassName = "rector-node-slot--selected";
        const string ActiveClassName = "rector-node-slot--active";

        public Vector2 ConnectorPosition => root.worldBound.center;

        public OutputSlotView(VisualElement templateContainer)
        {
            root = templateContainer.Q<VisualElement>("output-slot");
            nameLabel = root.Q<Label>("name-label");
        }

        public IDisposable Bind(OutputSlot slot)
        {
            nameLabel.text = slot.Name;
            root.AddToClassList(SelectedClassName);

            switch (slot)
            {
                case OutputSlot<bool> boolSlot:
                    return new CompositeDisposable(
                        boolSlot.Observable().Subscribe(x => root.EnableInClassList(ActiveClassName, x)),
                        slot.Selected.Subscribe(x => root.EnableInClassList(SelectedClassName, x))
                    );
                case OutputSlot<int> intSlot:
                    return new CompositeDisposable(
                        intSlot.Observable().Subscribe(x => root.EnableInClassList(ActiveClassName, x != 0)),
                        slot.Selected.Subscribe(x => root.EnableInClassList(SelectedClassName, x))
                    );
                case OutputSlot<float> floatSlot:
                    return new CompositeDisposable(
                        floatSlot.Observable().Subscribe(x => root.EnableInClassList(ActiveClassName, x != 0)),
                        slot.Selected.Subscribe(x => root.EnableInClassList(SelectedClassName, x))
                    );
                case OutputSlot<Unit> unitSlot:
                    return new CompositeDisposable(
                        unitSlot.Observable().Subscribe(x => root.EnableInClassList(ActiveClassName, true)),
                        unitSlot.Observable().DebounceFrame(InputSlotView.DebounceFrameCount).Subscribe(x => root.EnableInClassList(ActiveClassName, false)),
                        unitSlot.Selected.Subscribe(x => root.EnableInClassList(SelectedClassName, x))
                    );
            }

            return slot.Selected.Subscribe(x => root.EnableInClassList(SelectedClassName, x));
        }

        public void AddTo(VisualElement parent)
        {
            parent.Add(root);
        }
    }
}
