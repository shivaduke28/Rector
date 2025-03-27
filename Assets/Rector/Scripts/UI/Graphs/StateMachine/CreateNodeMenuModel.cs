using System;
using System.Collections.Generic;
using R3;

namespace Rector.UI.Graphs.StateMachine
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
        public readonly ReactiveProperty<NodeCategory> Category = new(NodeCategory.Vfx);
        readonly NodeTemplateRepository nodeTemplateRepository;
        readonly Graph graph;
        readonly Action onExit;

        public readonly List<RectorButtonState> CategoryButtons;
        readonly Dictionary<NodeCategory, List<RectorButtonState>> nodeButtons = new();

        int index;
        int subIndex;

        public CreateNodeMenuModel(NodeTemplateRepository nodeTemplateRepository, Graph graph, Action onExit)
        {
            this.nodeTemplateRepository = nodeTemplateRepository;
            this.graph = graph;
            this.onExit = onExit;
            CategoryButtons = new List<RectorButtonState>
            {
                new("Vfx", () => ChangeCategory(NodeCategory.Vfx)),
                new("Camera", () => ChangeCategory(NodeCategory.Camera)),
                new("Event", () => ChangeCategory(NodeCategory.Event)),
                new("Operator", () => ChangeCategory(NodeCategory.Operator)),
                new("Math", () => ChangeCategory(NodeCategory.Math)),
                new("Scene", () => ChangeCategory(NodeCategory.Scene)),
                new("System", () => ChangeCategory(NodeCategory.System)),
            };
        }

        void ChangeCategory(NodeCategory category)
        {
            if (State.Value == ViewState.Main)
            {
                // itemsが空の場合はSubに遷移できない
                var items = GetItems(category);
                if (items.Count == 0) return;

                Category.Value = category;
                State.Value = ViewState.Sub;
                subIndex = 0;
                items[subIndex].IsFocused.Value = true;
            }
        }

        void LoadButtons()
        {
            foreach (var buttons in nodeButtons.Values)
            {
                buttons.Clear();
            }

            foreach (var template in nodeTemplateRepository.GetAll())
            {
                GetItems(template.Category).Add(new RectorButtonState(template.Name, () => graph.Add(template.Factory(NodeId.Generate()))));
            }
        }

        public void Enter()
        {
            // 必要ならTemplateRepoのDirtyチェックをすることで無駄を削れる
            LoadButtons();
            Category.ForceNotify();
            Visible.Value = true;
            index = 0;
            subIndex = 0;
            CategoryButtons[index].IsFocused.Value = true;
        }

        void Exit()
        {
            CategoryButtons[index].IsFocused.Value = false;
            Visible.Value = false;
            onExit.Invoke();
        }

        public void Submit()
        {
            if (State.Value == ViewState.Main)
            {
                CategoryButtons[index].OnClick.Invoke();
            }
            else
            {
                GetItems(Category.Value)[subIndex].OnClick.Invoke();
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
                GetItems(Category.Value)[subIndex].IsFocused.Value = false;
                subIndex = 0;
                State.Value = ViewState.Main;
            }
        }

        public void Navigate(bool next)
        {
            if (State.Value == ViewState.Main)
            {
                CategoryButtons[index].IsFocused.Value = false;
                index = (index + (next ? 1 : -1) + CategoryButtons.Count) % CategoryButtons.Count;
                CategoryButtons[index].IsFocused.Value = true;
            }
            else
            {
                GetItems(Category.Value)[subIndex].IsFocused.Value = false;
                subIndex = (subIndex + (next ? 1 : -1) + GetItems(Category.Value).Count) %
                           GetItems(Category.Value).Count;
                GetItems(Category.Value)[subIndex].IsFocused.Value = true;
            }
        }

        public List<RectorButtonState> GetItems(NodeCategory category)
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
