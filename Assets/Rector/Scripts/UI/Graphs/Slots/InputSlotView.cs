using System;
using R3;
using UnityEngine;
using UnityEngine.UIElements;

namespace Rector.UI.Graphs.Slots
{
    public sealed class InputSlotView
    {
        public const int DebounceFrameCount = 6;
        readonly VisualElement root;
        readonly Label nameLabel;
        const string SelectedClassName = "rector-node-slot--selected";
        const string TargetClassName = "rector-node-slot--target";
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
            // ソース選択とターゲット指名は型に依らないのでここでまとめて張る
            var selection = new CompositeDisposable(
                slot.Selected.Subscribe(x => root.EnableInClassList(SelectedClassName, x)),
                slot.IsTarget.Subscribe(x => root.EnableInClassList(TargetClassName, x))
            );

            switch (slot)
            {
                case InputSlot<bool> boolSlot:
                    return new CompositeDisposable(
                        selection,
                        boolSlot.Observable().Subscribe(x => root.EnableInClassList(ActiveClassName, x))
                    );
                case InputSlot<Unit> unitSlot:
                    return new CompositeDisposable(
                        selection,
                        unitSlot.Observable().Subscribe(x => root.EnableInClassList(ActiveClassName, true)),
                        unitSlot.Observable().DebounceFrame(DebounceFrameCount).Subscribe(x => root.EnableInClassList(ActiveClassName, false))
                    );
                case InputSlot<int> intSlot:
                    return new CompositeDisposable(
                        selection,
                        intSlot.Observable().Subscribe(x => root.EnableInClassList(ActiveClassName, x != 0))
                    );
                case InputSlot<float> floatSlot:
                    return new CompositeDisposable(
                        selection,
                        floatSlot.Observable().Subscribe(x => root.EnableInClassList(ActiveClassName, x != 0))
                    );
            }

            return selection;
        }

        public void AddTo(VisualElement parent)
        {
            parent.Add(root);
        }
    }
}
