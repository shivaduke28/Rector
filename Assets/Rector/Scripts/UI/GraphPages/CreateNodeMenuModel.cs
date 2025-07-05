using System;
using System.Collections.Generic;
using R3;
using Rector.UI.Graphs;

namespace Rector.UI.GraphPages
{
    public sealed class CreateNodeMenuModel
    {
        public enum ViewState
        {
            Main,
            Sub,
        }

        public readonly ReactiveProperty<bool> Visible = new(false);
        public readonly ReactiveProperty<ViewState> State = new(ViewState.Main);
        readonly GraphPage graphPage;
        readonly NodeTemplateRepository nodeTemplateRepository;
        readonly Action onExit;

        public readonly List<RectorButtonState> CategoryButtons = new();
        readonly Dictionary<NodeCategory, List<RectorButtonState>> nodeButtons = new();
        readonly List<NodeCategory> categories = new();

        public int CategoryIndex { get; private set; }
        int subIndex;

        public CreateNodeMenuModel(GraphPage graphPage, NodeTemplateRepository nodeTemplateRepository, Action onExit)
        {
            this.graphPage = graphPage;
            this.nodeTemplateRepository = nodeTemplateRepository;
            this.onExit = onExit;
        }

        void ChangeCategory(NodeCategory category)
        {
            if (State.Value == ViewState.Main)
            {
                // itemsが空の場合はSubに遷移できない
                var items = GetItems(category);
                if (items.Count == 0) return;

                CategoryIndex = categories.IndexOf(category);
                State.Value = ViewState.Sub;
                subIndex = 0;
                items[subIndex].IsFocused.Value = true;
            }
        }

        // 必要ならTemplateRepoのDirtyチェックをすることで無駄を削れる
        void LoadButtons()
        {
            categories.Clear();
            CategoryButtons.Clear();
            var categoryNodeSet = nodeTemplateRepository.CategoryNodeSet;
            foreach (var (category, nodeTemplates) in categoryNodeSet)
            {
                var categoryButton = new RectorButtonState(category.ToString(), () => ChangeCategory(category));
                CategoryButtons.Add(categoryButton);
                categories.Add(category);

                if (nodeButtons.TryGetValue(category, out var buttons))
                {
                    buttons.Clear();
                }
                else
                {
                    buttons = new List<RectorButtonState>();
                    nodeButtons.Add(category, buttons);
                }

                foreach (var nodeTemplate in nodeTemplates)
                {
                    var button = new RectorButtonState(nodeTemplate.Name, () =>
                    {
                        graphPage.Graph.AddNode(nodeTemplate.Create(NodeId.Generate()));
                        graphPage.Sort();
                    });
                    buttons.Add(button);
                }
            }
        }

        public void Enter()
        {
            LoadButtons();
            CategoryIndex = 0;
            subIndex = 0;
            State.ForceNotify();
            Visible.Value = true;
            CategoryButtons[CategoryIndex].IsFocused.Value = true;
        }

        void Exit()
        {
            CategoryButtons[CategoryIndex].IsFocused.Value = false;
            Visible.Value = false;
            onExit.Invoke();
        }

        public void Submit()
        {
            if (State.Value == ViewState.Main)
            {
                CategoryButtons[CategoryIndex].OnClick.Invoke();
            }
            else
            {
                GetItems(CategoryIndex)[subIndex].OnClick.Invoke();
            }
        }

        public void Cancel()
        {
            if (State.Value == ViewState.Main)
            {
                Exit();
            }
            else
            {
                GetItems(CategoryIndex)[subIndex].IsFocused.Value = false;
                subIndex = 0;
                State.Value = ViewState.Main;
            }
        }

        public void Navigate(bool next)
        {
            if (State.Value == ViewState.Main)
            {
                CategoryButtons[CategoryIndex].IsFocused.Value = false;
                CategoryIndex = (CategoryIndex + (next ? 1 : -1) + CategoryButtons.Count) % CategoryButtons.Count;
                CategoryButtons[CategoryIndex].IsFocused.Value = true;
            }
            else
            {
                var items = GetItems(CategoryIndex);
                items[subIndex].IsFocused.Value = false;
                subIndex = (subIndex + (next ? 1 : -1) + items.Count) % items.Count;
                items[subIndex].IsFocused.Value = true;
            }
        }

        public List<RectorButtonState> GetItems(int index)
        {
            return GetItems(categories[index]);
        }

        List<RectorButtonState> GetItems(NodeCategory category)
        {
            if (!nodeButtons.TryGetValue(category, out var buttons))
            {
                buttons = new List<RectorButtonState>();
                nodeButtons.Add(category, buttons);
            }

            return buttons;
        }
    }
}
