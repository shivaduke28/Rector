using System;
using System.Collections.Generic;
using R3;
using Rector.UI.GraphPages.NodeParameters;
using Rector.UI.Graphs;
using Rector.UI.Graphs.Nodes;
using Rector.UI.Graphs.Slots;
using Rector.UI.LayeredGraphDrawing;
using UnityEngine;
using UnityEngine.UIElements;

namespace Rector.UI.GraphPages
{
    public sealed class GraphPage : IInitializable, IDisposable
    {
        readonly ReactiveProperty<bool> isVisible = new(false);
        public readonly ReactiveProperty<GraphPageState> State = new(GraphPageState.NodeSelection);
        GraphPageState lastState;

        // ReactivePropertyやめられそう
        public readonly ReactiveProperty<Node> SelectedNode = new();
        public readonly ReactiveProperty<ISlot> SelectedSlot = new();
        public readonly ReactiveProperty<Node> TargetNode = new();
        public readonly ReactiveProperty<ISlot> TargetSlot = new();

        public Observable<Unit> OpenScenePage => graphInputAction.OpenScene.Where(_ => State.Value == GraphPageState.NodeSelection);
        public Observable<Unit> OpenSystemPage => graphInputAction.OpenSystem.Where(_ => State.Value == GraphPageState.NodeSelection);

        readonly Dictionary<GraphPageState, GraphPageInputHandler> stateMap = new();

        GraphPageInputHandler CurrentInputHandler => stateMap[State.Value];

        const string RootName = "graph-page";
        readonly VisualElement root;

        readonly GraphInputAction graphInputAction;

        public readonly LayeredGraph Graph;

        readonly CreateNodeMenuModel createNodeMenuModel;
        readonly CreateNodeMenuView createNodeMenuView;
        readonly HoldGuideModel holdGuideModel = new();
        readonly HoldGuideView holdGuideView = new();
        readonly NodeParameterView nodeParameterView;
        readonly NodeParameterModel nodeParameterModel;

        readonly GraphContentTransformer graphContentTransformer;
        readonly GraphSorter graphSorter = new();

        readonly CompositeDisposable disposable = new();

        // for hud
        public readonly ReactiveProperty<int> NodeCount = new();
        public readonly ReactiveProperty<int> EdgeCount = new();
        public readonly ReactiveProperty<int> LayerCount = new();
        public readonly ReactiveProperty<int> DummyNodeCount = new();
        public readonly ReactiveProperty<int> Type1ConflictCount = new();

        public GraphPage(VisualElement container,
            GraphInputAction graphInputAction,
            NodeTemplateRepository nodeTemplateRepository)
        {
            this.graphInputAction = graphInputAction;

            // VisualElements
            root = container.Q<VisualElement>(RootName);
            var graphMask1 = root.Q<VisualElement>("graph-mask");
            var graphContent1 = graphMask1.Q<VisualElement>("graph-content");
            var nodeRoot1 = graphContent1.Q<VisualElement>("node-root");
            var edgeRoot1 = graphContent1.Q<VisualElement>("edge-root");
            nodeParameterView = new NodeParameterView(root.Q<VisualElement>(NodeParameterView.RootName));
            nodeParameterModel = new NodeParameterModel(this);
            createNodeMenuView = new CreateNodeMenuView(root.Q<VisualElement>(CreateNodeMenuView.RootName));
            createNodeMenuModel = new CreateNodeMenuModel(this, nodeTemplateRepository,
                () => State.Value = GraphPageState.NodeSelection);
            graphContent1.Add(holdGuideView);
            graphContentTransformer = new GraphContentTransformer(graphMask1, graphContent1, graphInputAction);

            Graph = new LayeredGraph(nodeRoot1, edgeRoot1);


            // state machine
            var nodeNavigator = new NodeNavigator(Graph);
            stateMap.Add(GraphPageState.NodeSelection, new NodeSelectionInputHandler(this, nodeNavigator));
            stateMap.Add(GraphPageState.SlotSelection, new SlotSelectionInputHandler(this));
            stateMap.Add(GraphPageState.TargetNodeSelection, new TargetNodeSelectionInputHandler(this, nodeNavigator));
            stateMap.Add(GraphPageState.TargetSlotSelection, new TargetSlotSelectionInputHandler(this));
            stateMap.Add(GraphPageState.NodeCreation, new NodeCreationInputHandler(createNodeMenuView));
            stateMap.Add(GraphPageState.NodeParameter, new NodeParameterInputHandler(nodeParameterView));
        }

        public void Enter()
        {
            isVisible.Value = true;
            graphInputAction.Enable();
        }

        public void Exit()
        {
            isVisible.Value = false;
            graphInputAction.Disable();
        }

        void IInitializable.Initialize()
        {
            isVisible.Subscribe(x => root.style.display = x ? DisplayStyle.Flex : DisplayStyle.None).AddTo(disposable);

            createNodeMenuView.Bind(createNodeMenuModel).AddTo(disposable);
            graphContentTransformer.Initialize();
            graphContentTransformer.AddTo(disposable);

            State.Where(x => x == GraphPageState.NodeCreation)
                .Subscribe(_ =>
                {
                    createNodeMenuModel.Enter();
                    var position = new Vector2(60, 30);
                    if (SelectedNode.Value is { } selectedNode && Graph.TryGetNode(selectedNode.Id, out var selectedNodeView))
                    {
                        position = selectedNodeView.Position + new Vector2(selectedNodeView.Width + 20, 40);
                    }

                    createNodeMenuView.SetPosition(position);
                }).AddTo(disposable);
            State.Where(x => x == GraphPageState.NodeParameter)
                .Subscribe(_ => nodeParameterModel.Enter()).AddTo(disposable);

            graphInputAction.Navigate.Subscribe(x => CurrentInputHandler.Navigate(x)).AddTo(disposable);
            graphInputAction.Submit.Subscribe(_ => CurrentInputHandler.Submit()).AddTo(disposable);
            graphInputAction.Cancel.Subscribe(_ => CurrentInputHandler.Cancel()).AddTo(disposable);
            graphInputAction.Action.Subscribe(_ => CurrentInputHandler.Action()).AddTo(disposable);
            graphInputAction.AddNode.Subscribe(_ => CurrentInputHandler.AddNode()).AddTo(disposable);
            graphInputAction.Mute.Subscribe(_ => CurrentInputHandler.Mute()).AddTo(disposable);
            graphInputAction.OpenNodeParameter.Subscribe(_ => CurrentInputHandler.OpenNodeParameter()).AddTo(disposable);
            graphInputAction.CloseNodeParameter.Subscribe(_ => CurrentInputHandler.CloseNodeParameter()).AddTo(disposable);
            graphInputAction.RemoveNode.Subscribe(x => CurrentInputHandler.RemoveNode(x)).AddTo(disposable);
            graphInputAction.RemoveEdge.Subscribe(x => CurrentInputHandler.RemoveEdge(x)).AddTo(disposable);

            holdGuideView.Bind(holdGuideModel).AddTo(disposable);
            nodeParameterView.Bind(nodeParameterModel).AddTo(disposable);

            // SortでNodeViewのWidthを使用するので1F待機する
            Observable.EveryUpdate(UnityFrameProvider.PostLateUpdate)
                .Where(_ => shouldSort)
                .Subscribe(_ => SortInternal()).AddTo(disposable);
        }

        public void SelectNode(Node node)
        {
            if (SelectedNode.Value is { } old)
            {
                old.Selected.Value = false;
            }

            if (node != null)
            {
                node.Selected.Value = true;
            }

            SelectedNode.Value = node;
        }

        public void SelectSlot(ISlot slot)
        {
            if (SelectedSlot.Value is { } old)
            {
                old.Selected.Value = false;
            }

            if (slot != null)
            {
                slot.Selected.Value = true;
            }

            SelectedSlot.Value = slot;
        }

        public void SetTargetNode(Node node)
        {
            if (TargetNode.Value is { } old && old != SelectedNode.Value)
            {
                old.Selected.Value = false;
            }

            if (node != null)
            {
                node.Selected.Value = true;
            }

            TargetNode.Value = node;
        }

        public void SetTargetSlot(ISlot slot)
        {
            if (TargetSlot.Value is { } old)
            {
                old.Selected.Value = false;
            }

            if (slot != null)
            {
                slot.Selected.Value = true;
            }

            TargetSlot.Value = slot;
        }


        public void ShowHoldNextToSelected()
        {
            if (SelectedNode.Value is { } selectedNode && Graph.TryGetNode(selectedNode.Id, out var selectedLayeredNode))
            {
                holdGuideModel.Position.Value = selectedLayeredNode.Position - new Vector2(30, 0);
                holdGuideModel.Visible.Value = true;
            }
        }

        public void HideHold()
        {
            holdGuideModel.Visible.Value = false;
        }

        public void RemoveSelectedNode()
        {
            if (SelectedNode.Value is { } selectedNode)
            {
                Graph.RemoveNode(selectedNode.Id);
                SelectSlot(null);
                SelectNode(null);
                Sort();
                State.Value = GraphPageState.NodeSelection;
            }
        }

        public void Sort()
        {
            shouldSort = true;
        }

        bool shouldSort;

        void SortInternal()
        {
            shouldSort = false;
            var result = graphSorter.Sort(NodeViews.Values, edgeViews.Values);
            Layers.Clear();
            foreach (var nodeView in NodeViews.Values)
            {
                while (Layers.Count <= nodeView.LayerIndex)
                {
                    Layers.Add(new List<NodeView>());
                }

                Layers[nodeView.LayerIndex].Add(nodeView);
            }

            foreach (var layer in Layers)
            {
                layer.Sort((x, y) => x.IndexInLayer.CompareTo(y.IndexInLayer));
            }

            NodeCount.Value = Graph.NodeCount;
            EdgeCount.Value = Graph.EdgeCount;
            LayerCount.Value = result.LayerCount;
            DummyNodeCount.Value = result.DummyNodeCount;
            Type1ConflictCount.Value = result.Type1ConflictCount;
        }

        void IDisposable.Dispose()
        {
            disposable.Dispose();
        }
    }
}
