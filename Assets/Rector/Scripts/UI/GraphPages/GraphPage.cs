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

#nullable enable

namespace Rector.UI.GraphPages
{
    public sealed class GraphPage : IInitializable, IDisposable
    {
        readonly ReactiveProperty<bool> isVisible = new(false);
        public readonly ReactiveProperty<GraphPageState> State = new(GraphPageState.NodeSelection);

        public LayeredNode? SelectedNode { get; private set; }
        public ISlot? SelectedSlot { get; private set; }
        public LayeredNode? TargetNode { get; private set; }
        public ISlot? TargetSlot { get; private set; }

        public Observable<Unit> OpenScenePage => graphInputAction.OpenScene.Where(_ => State.Value == GraphPageState.NodeSelection);
        public Observable<Unit> OpenSystemPage => graphInputAction.OpenSystem.Where(_ => State.Value == GraphPageState.NodeSelection);

        readonly Dictionary<GraphPageState, GraphPageInputHandler> stateMap = new();

        GraphPageInputHandler CurrentInputHandler => stateMap[State.Value];

        const string RootName = "graph-page";
        readonly VisualElement root;

        readonly GraphInputAction graphInputAction;

        public readonly LayeredGraph Graph;
        public readonly NodeGroups Groups = new();
        public readonly InputGuideSettings GuideSettings = new();

        readonly CreateNodeMenuModel createNodeMenuModel;
        readonly CreateNodeMenuView createNodeMenuView;
        readonly HoldGuideModel holdGuideModel = new();
        readonly HoldGuideView holdGuideView = new();
        readonly NodeParameterView nodeParameterView;
        readonly NodeParameterModel nodeParameterModel;

        readonly GraphContentTransformer graphContentTransformer;
        readonly GroupGuideView groupGuideView;
        readonly InputGuideView inputGuideView = new();
        readonly GraphSorter graphSorter;
        readonly NodeNavigator nodeNavigator;

        readonly CompositeDisposable disposable = new();

        // for hud
        public readonly ReactiveProperty<int> NodeCount = new();
        public readonly ReactiveProperty<int> EdgeCount = new();
        public readonly ReactiveProperty<int> LayerCount = new();
        public readonly ReactiveProperty<int> DummyNodeCount = new();
        public readonly ReactiveProperty<int> Type1ConflictCount = new();

        public bool IsNodeParameterOpen => graphInputAction.IsNodeParameterOpen;

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
            groupGuideView = new GroupGuideView(graphMask1.Q<VisualElement>(GroupGuideView.RootName), Groups);
            graphContentTransformer = new GraphContentTransformer(graphMask1, graphContent1, graphInputAction, groupGuideView);
            // maskではなくページ基準の右下に固定する。mask基準だとパラメータパネルの
            // 開閉でmask幅が変わりガイドが左右に動いてしまう。描画順はガイドが
            // node-detailより上だが、パネルは背景無し・コンテンツ高なので実害はない
            root.Add(inputGuideView);

            Graph = new LayeredGraph(nodeRoot1, edgeRoot1);
            graphSorter = new GraphSorter(Graph, Groups);


            // state machine
            nodeNavigator = new NodeNavigator(Graph, Groups);
            stateMap.Add(GraphPageState.NodeSelection, new NodeSelectionInputHandler(this, nodeNavigator));
            stateMap.Add(GraphPageState.SlotSelection, new SlotSelectionInputHandler(this));
            stateMap.Add(GraphPageState.TargetNodeSelection, new TargetNodeSelectionInputHandler(this, nodeNavigator));
            stateMap.Add(GraphPageState.TargetSlotSelection, new TargetSlotSelectionInputHandler(this));
            stateMap.Add(GraphPageState.NodeCreation, new NodeCreationInputHandler(createNodeMenuView));
            stateMap.Add(GraphPageState.NodeParameter, new NodeParameterInputHandler(this, nodeParameterView));
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

            // 各パネルは自分の Exit 経路でしか閉じないので、State を外（CLI）から動かされると
            // 出しっぱなしになる。State から閉じる側も張っておく。
            State.Subscribe(x =>
            {
                if (x != GraphPageState.NodeCreation) createNodeMenuModel.Hide();
                if (x != GraphPageState.NodeParameter) nodeParameterModel.Hide();
            }).AddTo(disposable);

            State.Where(x => x == GraphPageState.NodeCreation)
                .Subscribe(_ =>
                {
                    createNodeMenuModel.Enter();
                    var position = new Vector2(60, 30);
                    if (SelectedNode is not null && Graph.TryGetNode(SelectedNode.Id, out var selectedNodeView))
                    {
                        position = selectedNodeView.Position + new Vector2(selectedNodeView.Width + 20, 40);
                    }

                    createNodeMenuView.SetPosition(position);
                }).AddTo(disposable);
            State.Where(x => x == GraphPageState.NodeParameter)
                .Subscribe(_ => nodeParameterModel.Enter()).AddTo(disposable);

            // グループ数が変わったら並べ直す。ノードのGroupは書き換えない（NodeGroups.Foldが畳む）ので、
            // 数を戻せば元の並びに戻る。
            Groups.Count.Subscribe(_ => Sort()).AddTo(disposable);

            graphInputAction.Navigate.Subscribe(x => CurrentInputHandler.Navigate(x)).AddTo(disposable);
            graphInputAction.MoveGroup.Subscribe(x => CurrentInputHandler.MoveGroup(x)).AddTo(disposable);
            graphInputAction.MoveNodeToGroup.Subscribe(x => CurrentInputHandler.MoveNodeToGroup(x)).AddTo(disposable);
            graphInputAction.Submit.Subscribe(_ => CurrentInputHandler.Submit()).AddTo(disposable);
            graphInputAction.Cancel.Subscribe(_ => CurrentInputHandler.Cancel()).AddTo(disposable);
            graphInputAction.Action.Subscribe(_ => CurrentInputHandler.Action()).AddTo(disposable);
            graphInputAction.AddNode.Subscribe(_ => CurrentInputHandler.AddNode()).AddTo(disposable);
            graphInputAction.Mute.Subscribe(_ => CurrentInputHandler.Mute()).AddTo(disposable);
            // R2で掴んでいる間、選択中ノードに矢印を出す。掴んだままSubmit等でステートが
            // 変わることもあるので、修飾キーとステートの両方に反応させる
            graphInputAction.GrabModifierHeld.Subscribe(_ => UpdateGrabIndicator()).AddTo(disposable);
            State.Subscribe(_ => UpdateGrabIndicator()).AddTo(disposable);
            // ロックは押した瞬間の画面位置をアンカーにする(中央へは寄せない)。以後の追従は
            // FollowLockedNodeの既存呼び出し(選択・ターゲット変更時)が拾う。
            graphInputAction.LockStarted
                .Subscribe(_ => graphContentTransformer.BeginLockFollow(GetFocusNodeForCurrentState())).AddTo(disposable);
            graphInputAction.LockEnded
                .Subscribe(_ => graphContentTransformer.EndLockFollow()).AddTo(disposable);
            graphInputAction.OpenNodeParameter.Subscribe(_ => CurrentInputHandler.OpenNodeParameter()).AddTo(disposable);
            graphInputAction.CloseNodeParameter.Subscribe(_ => CurrentInputHandler.CloseNodeParameter()).AddTo(disposable);
            graphInputAction.RemoveNode.Subscribe(x => CurrentInputHandler.RemoveNode(x)).AddTo(disposable);
            graphInputAction.RemoveEdge.Subscribe(x => CurrentInputHandler.RemoveEdge(x)).AddTo(disposable);

            holdGuideView.Bind(holdGuideModel).AddTo(disposable);
            nodeParameterView.Bind(nodeParameterModel).AddTo(disposable);
            inputGuideView.Bind(State, graphInputAction.GrabModifierHeld, graphInputAction.LockHeld, GuideSettings.Mode).AddTo(disposable);

            // SortでNodeViewのWidthを使用するので1F待機する
            Observable.EveryUpdate(UnityFrameProvider.PostLateUpdate)
                .Where(_ => isVisible.Value)
                .Where(_ => shouldSort)
                .Subscribe(_ => SortInternal()).AddTo(disposable);
        }

        /// <summary>
        /// ノードを追加する。新しいノードは選択中のノードと同じグループに入る。
        /// </summary>
        public void AddNode(NodeView nodeView)
        {
            Graph.AddNode(nodeView, SelectedNode?.Group ?? 0);
            Sort();
        }

        /// <summary>
        /// フォーカスを隣のグループへ移す。directionは-1か1。
        /// </summary>
        /// <remarks>
        /// ノードのないグループは飛ばし、端まで行ったらループする。
        /// </remarks>
        public void MoveActiveGroup(int direction)
        {
            var next = nodeNavigator.FindNodeInAdjacentGroup(SelectedNode, direction, Groups.CurrentCount);
            if (next != null)
            {
                SelectNode(next);
            }
        }

        /// <summary>
        /// 選択中のノードを隣のグループへ移す。directionは-1か1。
        /// </summary>
        public void MoveSelectedNodeToGroup(int direction)
        {
            if (SelectedNode is not { } node) return;

            // 起点はFoldした表示上のグループ。生のGroupを起点にすると、グループ数を超えた
            // 番号を持つノードが見た目と違う場所へ飛ぶ。
            var current = Groups.Fold(node.Group);
            var target = Groups.Wrap(current + direction);

            // 表示位置が変わらないなら生のGroupを触らない。グループ数1のときに畳んだ値で
            // 上書きすると、数を戻しても並びが復元できなくなる(Foldが生の値を保存する設計)。
            if (target == current) return;

            MoveNodeToGroup(node, target);
        }

        public void MoveNodeToGroup(LayeredNode node, int group)
        {
            if (group == node.Group) return;

            node.Group = group;
            Sort();
        }

        public void SelectNode(LayeredNode? node)
        {
            if (SelectedNode is { } old)
            {
                old.NodeView.Node.Selected.Value = false;
            }

            if (node != null)
            {
                node.NodeView.Node.Selected.Value = true;
                graphContentTransformer.FollowLockedNode(node);
            }

            SelectedNode = node;
            groupGuideView.SetActiveGroup(node is null ? -1 : Groups.Fold(node.Group));
            // 掴んだまま選択が動く経路(キーボードQ/Eのグループジャンプ等)でも矢印を追従させる
            UpdateGrabIndicator();
        }

        Node? grabbedNode;

        void UpdateGrabIndicator()
        {
            // MoveNodeToGroupが効くのはNodeSelectionだけなので、矢印もそこに限定する
            var next = graphInputAction.GrabModifierHeld.CurrentValue && State.Value == GraphPageState.NodeSelection
                ? SelectedNode?.NodeView.Node
                : null;
            if (ReferenceEquals(grabbedNode, next)) return;

            if (grabbedNode != null)
            {
                grabbedNode.IsGrabbed.Value = false;
            }

            grabbedNode = next;
            if (grabbedNode != null)
            {
                grabbedNode.IsGrabbed.Value = true;
            }
        }

        public void SelectSlot(ISlot? slot)
        {
            if (SelectedSlot is { } old)
            {
                old.Selected.Value = false;
            }

            if (slot != null)
            {
                slot.Selected.Value = true;
            }

            SelectedSlot = slot;
        }

        public void SetTargetNode(LayeredNode? node)
        {
            // IsTargetはターゲット専用フラグなので、旧ターゲットからは無条件に降ろす
            // (ソースの見た目はSelectedが別に持っている)
            if (TargetNode is { } old)
            {
                old.NodeView.Node.IsTarget.Value = false;
            }

            if (node != null)
            {
                node.NodeView.Node.IsTarget.Value = true;
                graphContentTransformer.FollowLockedNode(node);
            }
            else if (SelectedNode is not null)
            {
                graphContentTransformer.FollowLockedNode(SelectedNode);
            }

            TargetNode = node;
        }

        public void SetTargetSlot(ISlot? slot)
        {
            if (TargetSlot is { } old)
            {
                old.IsTarget.Value = false;
            }

            if (slot != null)
            {
                slot.IsTarget.Value = true;
            }

            TargetSlot = slot;
        }

        /// <summary>
        /// 選択・ターゲット・State をまとめて NodeSelection に揃える。
        /// </summary>
        /// <remarks>
        /// SelectNode を SetTargetNode より先に呼ぶこと。逆にすると SetTargetNode(null) の
        /// else 分岐が、これから外す（あるいは削除する）SelectedNode に向けて
        /// FollowLockedNode してしまう。
        /// </remarks>
        public void EnterNodeSelection(LayeredNode? node)
        {
            SelectSlot(null);
            SelectNode(node);
            SetTargetSlot(null);
            SetTargetNode(null);
            State.Value = GraphPageState.NodeSelection;
        }

        /// <summary>ノードのミュートをトグルしてログを残す。全ハンドラ共通の入り口。</summary>
        public void ToggleMute(LayeredNode? node)
        {
            if (node is not { NodeView: { Node: var target } }) return;

            var mute = !target.IsMuted.Value;
            target.IsMuted.Value = mute;
            RectorLogger.ToggleMute(target, mute);
        }

        /// <summary>
        /// ターゲットをソースに引き継いでスロット選択へ移る。ターゲット選択中の△/Cで、
        /// ソース選択まで戻らずに、いま指しているノードから続けて次のエッジを張るための操作。
        /// 昇格後のスロットは昇格前のソーススロットと同じ向きに揃える。A.out→B.inと繋いだ流れなら
        /// 昇格後もoutputを指すので、B→CをCONTINUEの連打だけで伸ばせる。
        /// </summary>
        /// <remarks>
        /// Target系のクリアはセッターを通さない。SetTargetNode(null)は旧SelectedNodeへ視点を
        /// 戻してしまうため。代わりにIsTargetはここで明示的に降ろす(降ろし忘れると表示が残留する)。
        /// 途中で抜けるとIsTargetが残留するので、早期returnは状態を書き換える前に済ませること。
        /// 向きの読み取りもSelectSlotより前に。SelectSlotは旧SelectedSlotを潰す。
        /// </remarks>
        public void PromoteTargetToSource()
        {
            switch (State.Value)
            {
                case GraphPageState.TargetNodeSelection:
                    {
                        // CLIからStateを直接動かされた場合に備えてnullを弾く
                        if (TargetNode is not { } target) return;
                        var node = target.NodeView.Node;
                        // SelectedSlotが無いのもCLI経由の異常系。従来通りinput優先に倒す
                        var direction = SelectedSlot?.Direction ?? SlotDirection.Input;
                        if (PickSlotInDirection(direction, node.InputSlots, node.OutputSlots) is not { } next) return;

                        node.IsTarget.Value = false;
                        TargetNode = null;
                        SelectNode(target);
                        SelectSlot(next);
                        State.Value = GraphPageState.SlotSelection;
                        break;
                    }
                case GraphPageState.TargetSlotSelection:
                    {
                        if (TargetNode is not { } target || TargetSlot is not { } targetSlot) return;
                        var node = target.NodeView.Node;
                        var direction = SelectedSlot?.Direction ?? targetSlot.Direction;
                        var next = PickSlotInDirection(direction, node.InputSlots, node.OutputSlots) ?? targetSlot;

                        node.IsTarget.Value = false;
                        targetSlot.IsTarget.Value = false;
                        TargetNode = null;
                        TargetSlot = null;
                        SelectNode(target);
                        SelectSlot(next);
                        State.Value = GraphPageState.SlotSelection;
                        break;
                    }
            }
        }

        /// <summary>
        /// 指定した向きのスロットの先頭を返す。その向きが空なら反対側の先頭に落とし、
        /// どちらも空なら null。
        /// </summary>
        static ISlot? PickSlotInDirection(SlotDirection direction, InputSlot[] inputSlots, OutputSlot[] outputSlots)
        {
            if (direction == SlotDirection.Output)
            {
                if (outputSlots.Length > 0) return outputSlots[0];
                return inputSlots.Length > 0 ? inputSlots[0] : null;
            }

            if (inputSlots.Length > 0) return inputSlots[0];
            return outputSlots.Length > 0 ? outputSlots[0] : null;
        }

        public bool DisconnectSlots(OutputSlot output, InputSlot input)
        {
            if (!Graph.RemoveEdge(new EdgeId(output, input))) return false;
            Sort();
            return true;
        }

        /// <param name="replaceEdgesOn">
        /// 差し替え接続（HUD で OpenNodeParameter を押しながら繋いだとき）に、
        /// 先に張られていたエッジを外すスロット。検証を通ったあとに外すので、
        /// 接続を断られた場合に既存のエッジだけ失うことがない。
        /// </param>
        public ConnectResult TryConnectSlots(OutputSlot output, InputSlot input, ISlot? replaceEdgesOn = null)
        {
            if (Graph.Edges.ContainsKey(new EdgeId(output, input))) return ConnectResult.AlreadyConnected;

            // ValidateLoop を CanConnect より先に見るのは、自分自身への接続を Incompatible ではなく
            // Loop として返すため。HUD でも自ノードをターゲットにできる(ターゲットカーソルは
            // ソースの上から始まる)ので、その場合はこの順序によって Loop として弾かれる。
            if (!Graph.ValidateLoop(output, input))
            {
                RectorLogger.LoopDetected(output.NodeId, input.NodeId);
                return ConnectResult.Loop;
            }

            if (!EdgeConnector.CanConnect(output, input)) return ConnectResult.Incompatible;

            if (replaceEdgesOn is not null)
            {
                Graph.RemoveEdgesFrom(replaceEdgesOn);
            }

            if (!EdgeConnector.TryConnect(output, input, out var edge))
            {
                // replaceEdgesOn の分だけグラフが変わっている可能性がある
                Sort();
                return ConnectResult.Failed;
            }

            Graph.AddEdge(edge);
            Sort();
            return ConnectResult.Connected;
        }

        public void ShowHoldNextToSelected()
        {
            if (SelectedNode is not null && Graph.TryGetNode(SelectedNode.Id, out var selectedLayeredNode))
            {
                holdGuideModel.Position.Value = selectedLayeredNode.Position - new Vector2(30, 0);
                holdGuideModel.Visible.Value = true;
            }
        }

        public void HideHold()
        {
            holdGuideModel.Visible.Value = false;
        }

        /// <summary>
        /// グラフを空にする。グラフのロードで作り直す前に呼ぶ。
        /// </summary>
        /// <remarks>
        /// EnterNodeSelection を先に通すのは、選択・ターゲット・掴み(grabbedNode)の参照を
        /// ノードが生きているうちに外すため。RemoveSelectedNode と同じ理由。
        /// </remarks>
        public void ClearGraph()
        {
            EnterNodeSelection(null);
            Graph.ClearNodes();
            Sort();
        }

        public void RemoveSelectedNode()
        {
            if (SelectedNode is not { } node) return;

            // NodeView が生きているうちに選択とターゲットを外す
            EnterNodeSelection(null);
            Graph.RemoveNode(node.Id);
            Sort();
        }

        public void Sort()
        {
            shouldSort = true;
        }

        bool shouldSort;

        /// <summary>幅が解決しないまま毎フレームSortし続けないための上限。</summary>
        const int MaxWidthRetries = 3;
        int widthRetryCount;

        void SortInternal()
        {
            shouldSort = false;
            var result = graphSorter.Sort();

            NodeCount.Value = Graph.NodeCount;
            EdgeCount.Value = Graph.EdgeCount;
            LayerCount.Value = result.LayerCount;
            DummyNodeCount.Value = result.DummyNodeCount;
            Type1ConflictCount.Value = result.Type1ConflictCount;

            // グループ数を変えて選択ノードの表示先が変わった場合もここで追随する
            groupGuideView.SetActiveGroup(SelectedNode is null ? -1 : Groups.Fold(SelectedNode.Group));

            // 幅が未解決のまま並べてしまったので、解決を待って次のフレームでやり直す。
            // 単なるフラグにすると、やり直し待ちの間に足されたノードが2度目のSortをもらえない。
            // 連続して解決しない場合だけ諦める。
            if (result.HasUnresolvedWidth && widthRetryCount < MaxWidthRetries)
            {
                widthRetryCount++;
                Sort();
            }
            else
            {
                widthRetryCount = 0;
            }

            if (GetFocusNodeForCurrentState() is { } focus)
            {
                graphContentTransformer.FollowLockedNode(focus);
            }
        }

        /// <summary>Stateに応じた「いま操作しているノード」。ターゲット選択中はターゲット側。</summary>
        LayeredNode? GetFocusNodeForCurrentState() =>
            State.Value switch
            {
                GraphPageState.TargetNodeSelection or GraphPageState.TargetSlotSelection => TargetNode ?? SelectedNode,
                _ => SelectedNode,
            };

        void IDisposable.Dispose()
        {
            disposable.Dispose();
        }
    }
}
