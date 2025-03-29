using System;
using System.Collections.Generic;
using System.Linq;
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

        public readonly ReactiveProperty<Node> SelectedNode = new();
        public readonly ReactiveProperty<ISlot> SelectedSlot = new();
        public readonly ReactiveProperty<Node> TargetNode = new();
        public readonly ReactiveProperty<ISlot> TargetSlot = new();

        public Observable<Unit> OpenScenePage => graphInputAction.OpenScene.Where(_ => State.Value == GraphPageState.NodeSelection);
        public Observable<Unit> OpenSystemPage => graphInputAction.OpenSystem.Where(_ => State.Value == GraphPageState.NodeSelection);

        public readonly Graph Graph = new();
        readonly Dictionary<GraphPageState, GraphPageInputHandler> stateMap = new();

        GraphPageInputHandler CurrentInputHandler => stateMap[State.Value];

        const string RootName = "graph-page";
        readonly VisualElement root;
        readonly VisualElement graphMask;
        readonly VisualElement graphContent;
        readonly VisualElement nodeRoot;
        readonly VisualElement edgeRoot;
        readonly GraphInputAction graphInputAction;
        public readonly List<List<NodeView>> Layers = new();
        public readonly Dictionary<NodeId, NodeView> NodeViews = new();
        readonly Dictionary<EdgeId, EdgeView> edgeViews = new();

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
            graphMask = root.Q<VisualElement>("graph-mask");
            graphContent = graphMask.Q<VisualElement>("graph-content");
            nodeRoot = graphContent.Q<VisualElement>("node-root");
            edgeRoot = graphContent.Q<VisualElement>("edge-root");
            nodeParameterView = new NodeParameterView(root.Q<VisualElement>(NodeParameterView.RootName));
            nodeParameterModel = new NodeParameterModel(this);
            createNodeMenuView = new CreateNodeMenuView(root.Q<VisualElement>(CreateNodeMenuView.RootName));
            createNodeMenuModel = new CreateNodeMenuModel(this, nodeTemplateRepository,
                () => State.Value = GraphPageState.NodeSelection);
            graphContent.Add(holdGuideView);
            graphContentTransformer = new GraphContentTransformer(graphMask, graphContent, graphInputAction);

            // state machine
            stateMap.Add(GraphPageState.NodeSelection, new NodeSelectionInputHandler(this));
            stateMap.Add(GraphPageState.SlotSelection, new SlotSelectionInputHandler(this));
            stateMap.Add(GraphPageState.TargetNodeSelection, new TargetNodeSelectionInputHandler(this));
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
            Graph.OnNodeRemoved.Subscribe(node => OnNodeRemoved(node.Id)).AddTo(disposable);
            Graph.OnEdgeAdded.Subscribe(OnEdgeAdded).AddTo(disposable);
            Graph.OnEdgeRemoved.Subscribe(OnEdgeRemoved).AddTo(disposable);

            createNodeMenuView.Bind(createNodeMenuModel).AddTo(disposable);
            graphContentTransformer.Initialize();
            graphContentTransformer.AddTo(disposable);

            State.Where(x => x == GraphPageState.NodeCreation)
                .Subscribe(_ =>
                {
                    createNodeMenuModel.Enter();
                    var position = new Vector2(60, 30);
                    if (SelectedNode.Value is { } selectedNode &&
                        NodeViews.TryGetValue(selectedNode.Id, out var selectedNodeView))
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

        public void SelectTargetNode(Node node)
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

        public void SelectTargetSlot(ISlot slot)
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


        public void MoveGraphContentToNodeVisible(NodeView nodeView)
        {
            var nodeWorldBound = nodeView.WorldBound;
            var graphMaskWorldBound = graphMask.worldBound;

            var offset = nodeWorldBound.center - graphMaskWorldBound.center;
            var offsetSign = new Vector2(Mathf.Sign(offset.x), Mathf.Sign(offset.y));

            const float cornerOffsetX = 60f;
            const float cornerOffsetY = 30f;
            var nodeWorldCorner = nodeWorldBound.center
                                  + new Vector2(
                                      (nodeWorldBound.width * 0.5f + cornerOffsetX) * offsetSign.x,
                                      (nodeWorldBound.height * 0.5f + cornerOffsetY) * offsetSign.y);
            var maskWorldCorner = graphMaskWorldBound.center +
                                  new Vector2(
                                      graphMaskWorldBound.width * 0.5f * offsetSign.x,
                                      graphMaskWorldBound.height * 0.5f * offsetSign.y);

            if (graphMaskWorldBound.Contains(nodeWorldCorner))
            {
                return;
            }

            var diff = nodeWorldCorner - maskWorldCorner;
            var graphContentPosition = graphContent.transform.position;

            if (Mathf.Approximately(Mathf.Sign(diff.x), offsetSign.x))
            {
                graphContentPosition.x -= diff.x;
            }

            if (Mathf.Approximately(Mathf.Sign(diff.y), offsetSign.y))
            {
                graphContentPosition.y -= diff.y;
            }

            graphContent.transform.position = graphContentPosition;
        }

        public void ShowHoldNextTo(NodeView nodeView)
        {
            holdGuideModel.Position.Value = nodeView.Position - new Vector2(30, 0);
            holdGuideModel.Visible.Value = true;
        }

        public void HideHold()
        {
            holdGuideModel.Visible.Value = false;
        }

        public void RemoveSelectedNode()
        {
            if (SelectedNode.Value is { } selectedNode)
            {
                Graph.Remove(selectedNode.Id);
                SelectSlot(null);
                SelectNode(null);
                State.Value = GraphPageState.NodeSelection;
            }
        }


        void OnNodeRemoved(NodeId id)
        {
            if (NodeViews.Remove(id, out var nodeView))
            {
                nodeView.RemoveFrom(nodeRoot);
                nodeView.Dispose();

                RectorLogger.DeleteNode(nodeView.Node);
            }

            Sort();
        }

        void OnEdgeAdded(Edge edge)
        {
            var input = edge.InputSlot;
            var output = edge.OutputSlot;
            if (NodeViews.TryGetValue(input.NodeId, out var inputNodeView) &&
                NodeViews.TryGetValue(output.NodeId, out var outputNodeView))
            {
                var inputSlotView = inputNodeView.InputSlotViews[input.Index];
                var outputSlotView = outputNodeView.OutputSlotViews[output.Index];
                var edgeView = new EdgeView(outputSlotView, inputSlotView, edge);
                edgeRoot.Add(edgeView);
                edgeViews.Add(edge.Id, edgeView);

                RectorLogger.CreateEdge(edge, outputNodeView.Node, inputNodeView.Node);
            }

            Sort();
        }

        void OnEdgeRemoved(EdgeId id)
        {
            if (edgeViews.Remove(id, out var edgeView))
            {
                edgeRoot.Remove(edgeView);
                edgeView.Dispose();

                if (NodeViews.TryGetValue(id.OutputNodeId, out var output) && NodeViews.TryGetValue(id.InputNodeId, out var input))
                {
                    RectorLogger.DeleteEdge(edgeView.Edge, output.Node, input.Node);
                }
            }

            Sort();
        }

        void Sort()
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

            NodeCount.Value = NodeViews.Count;
            EdgeCount.Value = edgeViews.Count;
            LayerCount.Value = result.LayerCount;
            DummyNodeCount.Value = result.DummyNodeCount;
            Type1ConflictCount.Value = result.Type1ConflictCount;
        }

        public void AddNode(NodeView nodeView)
        {
            Graph.Add(nodeView.Node);
            nodeView.AddTo(nodeRoot);
            NodeViews.Add(nodeView.Node.Id, nodeView);

            if (Layers.Count > 0)
            {
                var layer = Layers[0];
                if (layer.Count > 0)
                {
                    var last = layer.Last();
                    if (last.Node.OutputSlots.All(s => s.ConnectedCount == 0))
                    {
                        var position = last.Position + new Vector2(last.Width + 20f, 0);
                        nodeView.Position = position;
                        nodeView.IndexInLayer = last.IndexInLayer + 1;
                    }
                }
            }

            Sort();
            RectorLogger.CreateNode(nodeView.Node);
        }

        void IDisposable.Dispose()
        {
            disposable.Dispose();
        }
    }
}
