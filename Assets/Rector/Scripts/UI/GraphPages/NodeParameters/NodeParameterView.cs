using System;
using R3;
using UnityEngine;
using UnityEngine.UIElements;

namespace Rector.UI.GraphPages.NodeParameters
{
    public sealed class NodeParameterView
    {
        public const string RootName = "node-detail";
        readonly VisualElement root;
        readonly Label nameLabel;
        readonly VisualElement propertyRoot;
        readonly SerialDisposable nodeDisposable = new();

        NodeParameterModel model;

        public NodeParameterView(VisualElement root)
        {
            this.root = root;
            nameLabel = root.Q<Label>("name-label");
            propertyRoot = root.Q<VisualElement>("property-root");
        }

        public IDisposable Bind(NodeParameterModel state)
        {
            model = state;
            return new CompositeDisposable(
                model.IsVisible.Subscribe(visible =>
                {
                    if (visible)
                        Show();
                    else
                        Hide();
                }),
                // 行の購読はSetup毎に差し替わるが、View自体を畳むときにも解けるようにしておく
                nodeDisposable
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
                foreach (var row in model.Rows)
                {
                    switch (row)
                    {
                        case ExposedFloatInputModel floatInput:
                            var floatInputView = new ExposedFloatInputView(VisualElementFactory.Instance.CreateExposedFloatSlot());
                            floatInputView.AddTo(propertyRoot);
                            floatInputView.Bind(floatInput).AddTo(disposable);
                            break;
                        case ExposedVector3HeaderRow vector3Header:
                            var vector3HeaderView = new ExposedVector3HeaderView(VisualElementFactory.Instance.CreateExposedVector3Header());
                            vector3HeaderView.AddTo(propertyRoot);
                            vector3HeaderView.Bind(vector3Header).AddTo(disposable);
                            break;
                        case ExposedVector3ComponentInputModel vector3Component:
                            var vector3ComponentView = new ExposedVector3ComponentInputView(VisualElementFactory.Instance.CreateExposedVector3ComponentSlot());
                            vector3ComponentView.AddTo(propertyRoot);
                            vector3ComponentView.Bind(vector3Component).AddTo(disposable);
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
                        default:
                            // 行を足してここを書き忘れると黙って消えるので、気づけるように落とす
                            throw new ArgumentOutOfRangeException(nameof(row), row, null);
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

        public void Action() => model.DoAction();

        public void CloseNodeParameter() => model.Close();
    }
}
