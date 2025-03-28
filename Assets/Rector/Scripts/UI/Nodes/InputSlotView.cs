using System;
using R3;
using UnityEngine;
using UnityEngine.UIElements;

namespace Rector.UI.Nodes
{
    public sealed class InputSlotView
    {
        public const int DebounceFrameCount = 6;
        readonly VisualElement root;
        readonly Label nameLabel;
        const string SelectedClassName = "rector-node-slot--selected";
        const string ActiveClassName = "rector-node-slot--active";
        public Vector2 ConnectorPosition => root.worldBound.center;

        public InputSlotView(VisualElement templateContainer)
        {
            root = templateContainer.Q<VisualElement>("input-slot");
            nameLabel = root.Q<Label>("name-label");
        }

        public IDisposable Bind(InputSlot slot)
        {
            nameLabel.text = slot.Name;
            switch (slot)
            {
                case InputSlot<bool> boolSlot:
                    return new CompositeDisposable(
                        boolSlot.Observable().Subscribe(x => root.EnableInClassList(ActiveClassName, x)),
                        boolSlot.Selected.Subscribe(x => root.EnableInClassList(SelectedClassName, x))
                    );
                case InputSlot<Unit> unitSlot:
                    return new CompositeDisposable(
                        unitSlot.Observable().Subscribe(x => root.EnableInClassList(ActiveClassName, true)),
                        unitSlot.Observable().DebounceFrame(DebounceFrameCount).Subscribe(x => root.EnableInClassList(ActiveClassName, false)),
                        unitSlot.Selected.Subscribe(x => root.EnableInClassList(SelectedClassName, x))
                    );
                case InputSlot<int> intSlot:
                    return new CompositeDisposable(
                        intSlot.Observable().Subscribe(x => root.EnableInClassList(ActiveClassName, x != 0)),
                        intSlot.Selected.Subscribe(x => root.EnableInClassList(SelectedClassName, x))
                    );
                case InputSlot<float> floatSlot:
                    return new CompositeDisposable(
                        floatSlot.Observable().Subscribe(x => root.EnableInClassList(ActiveClassName, x != 0)),
                        floatSlot.Selected.Subscribe(x => root.EnableInClassList(SelectedClassName, x))
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
