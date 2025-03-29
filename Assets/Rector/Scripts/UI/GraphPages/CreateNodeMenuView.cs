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

        readonly CompositeDisposable visibleDisposable = new();
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

            this.model.Visible.Subscribe(x =>
            {
                if (x)
                    Show();
                else
                    Hide();
            }).AddTo(disposable);

            this.model.State.Subscribe(state =>
            {
                if (state == CreateNodeMenuModel.ViewState.Main)
                {
                    subList.style.display = DisplayStyle.None;
                }
                else
                {
                    subList.style.display = DisplayStyle.Flex;
                    subList.Clear();
                    subListDisposable.Clear();
                    var category = this.model.CategoryIndex;
                    subList.style.marginTop = 18 * category;

                    // FIXME: そこそこアロケーションがあるので重い場合はキャッシュするとよい
                    // NodeTemplateRepo側がDirtyな場合だけShow時に初期化すればよいはず
                    foreach (var buttonState in this.model.GetItems(category))
                    {
                        var button = new RectorButton();
                        button.Bind(buttonState).AddTo(subListDisposable);
                        subList.Add(button);
                    }
                }
            }).AddTo(disposable);

            subListDisposable.AddTo(disposable);
            return disposable;
        }

        void Show()
        {
            mainList.Clear();
            visibleDisposable.Clear();
            foreach (var buttonState in model.CategoryButtons)
            {
                var button = new RectorButton();
                button.Bind(buttonState).AddTo(visibleDisposable);
                mainList.Add(button);
            }

            root.style.display = DisplayStyle.Flex;
        }

        void Hide()
        {
            visibleDisposable.Clear();
            root.style.display = DisplayStyle.None;
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
