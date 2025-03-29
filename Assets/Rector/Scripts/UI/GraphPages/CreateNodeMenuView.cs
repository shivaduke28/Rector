using System;
using R3;
using UnityEngine;
using UnityEngine.UIElements;

namespace Rector.UI.GraphPages
{
    public sealed class CreateNodeMenuView
    {
        public const string RootName = "create-node-menu";
        readonly VisualElement root;
        readonly VisualElement mainList;
        readonly VisualElement subList;

        CreateNodeMenuModel model;

        CompositeDisposable subListDisposable;

        public CreateNodeMenuView(VisualElement root)
        {
            this.root = root;
            mainList = root.Q<VisualElement>("create-node-menu-main-list");
            subList = root.Q<VisualElement>("create-node-menu-sub-list");
        }

        public IDisposable Bind(CreateNodeMenuModel model)
        {
            this.model = model;
            var disposable = new CompositeDisposable();
            subListDisposable = new CompositeDisposable();
            foreach (var buttonState in this.model.CategoryButtons)
            {
                var button = new RectorButton();
                button.Bind(buttonState).AddTo(disposable);
                mainList.Add(button);
            }

            this.model.Visible.Subscribe(x => root.style.visibility = x
                ? Visibility.Visible
                : Visibility.Hidden).AddTo(disposable);
            this.model.State.Subscribe(state => subList.style.visibility = state == CreateNodeMenuModel.ViewState.Main
                ? Visibility.Hidden
                : Visibility.Visible).AddTo(disposable);

            this.model.Category.Subscribe(category =>
            {
                subList.Clear();
                subListDisposable.Clear();
                subList.style.marginTop = 18 * (int)category;

                foreach (var buttonState in this.model.GetItems(category))
                {
                    var button = new RectorButton();
                    button.Bind(buttonState).AddTo(subListDisposable);
                    subList.Add(button);
                }

            }).AddTo(disposable);

            subListDisposable.AddTo(disposable);
            return disposable;
        }

        public void SetPosition(Vector2 position)
        {
            root.transform.position = position;
        }

        public void Navigate(Vector2 value)
        {
            if (value.y != 0)
            {
                model.Navigate(value.y < 0);
            }
        }

        public void Cancel() => model.Cancel();

        public void Submit() => model.Submit();
    }
}
