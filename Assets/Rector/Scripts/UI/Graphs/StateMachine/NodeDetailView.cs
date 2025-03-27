using System;
using R3;
using Rector.UI.NodeEdit;
using UnityEngine;
using UnityEngine.UIElements;

namespace Rector.UI.Graphs.StateMachine
{
    public sealed class NodeDetailView : IGraphPageState
    {
        public const string RootName = "node-detail";
        readonly VisualElement root;
        readonly Label nameLabel;
        readonly VisualElement propertyRoot;
        readonly SerialDisposable nodeDisposable = new();

        NodeDetailModel model;

        public NodeDetailView(VisualElement root)
        {
            this.root = root;
            nameLabel = root.Q<Label>("name-label");
            propertyRoot = root.Q<VisualElement>("property-root");
        }

        public IDisposable Bind(NodeDetailModel state)
        {
            model = state;
            return new CompositeDisposable(
                model.IsVisible.Subscribe(visible =>
                {
                    if (visible)
                    {
                        Show();
                    }
                    else
                    {
                        Hide();
                    }
                })
            );
        }

        void Show()
        {
            Setup();
            // GraphContentのサイズを変えたいのでDisplayを変える
            root.style.display = DisplayStyle.Flex;
        }

        void Setup()
        {
            var node = model.Node;
            if (node != null)
            {
                var disposable = new CompositeDisposable();
                nameLabel.text = node.Name;
                propertyRoot.Clear();
                var exposedInputs = model.ExposedInputs;
                foreach (var exposedInput in exposedInputs)
                {
                    switch (exposedInput)
                    {
                        case ExposedFloatInputModel floatInput:
                            var floatInputView = new ExposedFloatInputView(VisualElementFactory.Instance.CreateExposedFloatSlot());
                            floatInputView.AddTo(propertyRoot);
                            floatInputView.Bind(floatInput).AddTo(disposable);
                            break;
                        case ExposedIntInputModel intInput:
                            var intInputView = new ExposedIntInputView(VisualElementFactory.Instance.CreateExposedIntSlot());
                            intInputView.AddTo(propertyRoot);
                            intInputView.Bind(intInput).AddTo(disposable);
                            break;
                        case ExposedBoolInputModel boolInput:
                            var boolInputView = new ExposedBoolInputView(VisualElementFactory.Instance.CreateExposedBoolSlot());
                            boolInputView.AddTo(propertyRoot);
                            boolInputView.Bind(boolInput).AddTo(disposable);
                            break;
                        case ExposedCallbackInputModel callbackInput:
                            var callbackInputView = new ExposedCallbackInputView(VisualElementFactory.Instance.CreateExposedCallbackSlot());
                            callbackInputView.AddTo(propertyRoot);
                            callbackInputView.Bind(callbackInput).AddTo(disposable);
                            break;
                    }
                }

                nodeDisposable.Disposable = disposable;
            }
            else
            {
                nodeDisposable.Disposable = null;
                nameLabel.text = "No node selected";
                propertyRoot.Clear();
            }
        }

        void Hide()
        {
            root.style.display = DisplayStyle.None;
        }


        public void Navigate(Vector2 value)
        {
            if (value.sqrMagnitude == 0f) return;
            if (Mathf.Abs(value.x) > Mathf.Abs(value.y))
            {
                if (value.x > 0)
                {
                    model.Increment();
                }
                else
                {
                    model.Decrement();
                }
            }
            else
            {
                model.Navigate(value.y < 0);
            }
        }

        public void Cancel()
        {
        }

        public void Submit()
        {
        }

        public void Action1()
        {
            model.DoAction();
        }

        public void Action2()
        {
        }

        public void SubmitHoldStart()
        {
        }

        public void SubmitHoldCancel()
        {
        }

        public void SubmitHold()
        {
        }

        public void Action2HoldStart()
        {
        }

        public void Action2HoldCancel()
        {
        }

        public void Action2Hold()
        {
        }

        public void ToggleMute()
        {
        }
    }
}
