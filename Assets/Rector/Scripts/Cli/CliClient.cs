using System;
using System.Linq;
using Rector.Cameras;
using Rector.UI.GraphPages;
using Rector.UI.Graphs;
using Rector.UI.Graphs.Slots;
using Rector.UI.LayeredGraphDrawing;
using Rector.Vfx;

namespace Rector.Cli
{
    /// <summary>
    /// Unity CLI (com.unity.pipeline) から Rector を観測・操作するためのファサード。
    ///
    /// [CliCommand] は static メソッドしか登録できないが、Rector のシステムは
    /// RectorInstaller が組み立てるインスタンスとして存在する。そのため
    /// RectorInstaller が生成したこのクラスを Instance に登録し、コマンドは
    /// Instance 経由で呼ぶ。
    ///
    /// エディタの Play Mode でも Player ビルドでも同じコマンドが使える。
    /// アプリが動いていない間は Instance が null なので、各コマンドは
    /// 例外ではなく not_running を返す。
    /// </summary>
    public sealed partial class CliClient : IDisposable
    {
        public static CliClient Instance { get; private set; }

        public static void Register(CliClient client) => Instance = client;

        readonly GraphPage graphPage;
        readonly NodeTemplateRepository nodeTemplateRepository;
        readonly VfxManager vfxManager;
        readonly CameraManager cameraManager;
        readonly BGSceneManager bgSceneManager;

        public CliClient(
            GraphPage graphPage,
            NodeTemplateRepository nodeTemplateRepository,
            VfxManager vfxManager,
            CameraManager cameraManager,
            BGSceneManager bgSceneManager)
        {
            this.graphPage = graphPage;
            this.nodeTemplateRepository = nodeTemplateRepository;
            this.vfxManager = vfxManager;
            this.cameraManager = cameraManager;
            this.bgSceneManager = bgSceneManager;
        }

        public void Dispose()
        {
            if (ReferenceEquals(Instance, this)) Instance = null;
        }

        // ---------------------------------------------------------------- 観測

        object GetState() => new
        {
            camera = cameraManager.CurrentCamera.CurrentValue,
            scene = bgSceneManager.CurrentScene.CurrentValue,
            nodeCount = graphPage.Graph.NodeCount,
            edgeCount = graphPage.Graph.EdgeCount,
            columnCount = graphPage.Columns.CurrentCount,
            columns = graphPage.Columns.Bounds.Select(b => new { originX = b.OriginX, width = b.Width }).ToArray(),
            selectedNodeId = graphPage.SelectedNode?.Id.Value,
            pageState = graphPage.State.Value.ToString(),
        };

        object GetGraph() => new
        {
            nodes = Nodes().Select(ToNodeDto).ToArray(),
            edges = graphPage.Graph.Edges.Keys.Select(id => new
            {
                fromNode = id.OutputNodeId.Value,
                fromSlot = id.OutputSlotIndex,
                toNode = id.InputNodeId.Value,
                toSlot = id.InputSlotIndex,
            }).ToArray(),
        };

        object GetNodeTemplates() => nodeTemplateRepository.GetAll()
            .Select(t => new { category = t.Category.ToString(), name = t.Name })
            .ToArray();

        object GetVfx() => vfxManager.GetAllVfx()
            .Select(v => new { name = v.Name, category = v.Category.ToString() })
            .ToArray();

        object GetCameras() => new
        {
            current = cameraManager.CurrentCamera.CurrentValue,
            blendTime = cameraManager.BlendTime.Value,
            cameras = cameraManager.GetCameraBehaviours()
                .Select((c, i) => new { index = i, name = c.Name, active = c.IsActive.Value })
                .ToArray(),
        };

        object GetScenes() => new
        {
            current = bgSceneManager.CurrentScene.CurrentValue,
            scenes = bgSceneManager.GetScenes(),
        };

        // ------------------------------------------------------------ グラフ操作

        object CreateNode(string template)
        {
            var t = nodeTemplateRepository.GetAll().FirstOrDefault(x => x.Name == template);
            if (t == null) return Failure("unknown_template", $"No node template named '{template}'.");

            // HUD と同じ経路を通す。AddNode が「選択中のノードと同じカラムに入れる」まで見る。
            var nodeView = t.Create(NodeId.Generate());
            graphPage.AddNode(nodeView);
            return new { success = true, node = ToNodeDto(nodeView.Node.Id) };
        }

        object SetNodeColumn(uint id, int column)
        {
            if (!graphPage.Graph.TryGetNode(new NodeId(id), out var node)) return UnknownNode(id);

            var count = graphPage.Columns.CurrentCount;
            if (column < 0 || column >= count)
                return Failure("column_out_of_range", $"Column must be in [0, {count - 1}].");

            graphPage.MoveNodeToColumn(node, column);
            return new { success = true, node = ToNodeDto(node) };
        }

        // HUD ではノード削除だけが長押し (NodeSelectionInputHandler)。エッジ削除も
        // シーン切替も単押しなので、Rector 自身が「慎重にやる操作」と決めているのは
        // これだけ。undo もないため CLI からも一手間かける。
        object RemoveNode(uint id, bool confirm)
        {
            if (!graphPage.Graph.TryGetNode(new NodeId(id), out var node)) return UnknownNode(id);

            if (!confirm)
                return Failure("confirm_required", $"Removing node {id} ({node.NodeView.Node.Name}) cannot be undone. Pass confirm=true.");

            // HUD と同じ経路を通す。RemoveSelectedNode が選択・ターゲット・State の
            // 後始末までまとめて行うので、ここで個別に真似すると取りこぼす。
            graphPage.EnterNodeSelection(node);
            graphPage.RemoveSelectedNode();
            return new { success = true, removed = id };
        }

        object SelectNode(uint id)
        {
            if (!graphPage.Graph.TryGetNode(new NodeId(id), out var node)) return UnknownNode(id);

            // HUD はノード選択を NodeSelection / TargetNodeSelection でしか行わない。
            // 他の State のまま差し替えると SelectedSlot が前のノードのスロットを指したままに
            // なるので、State ごと揃える EnterNodeSelection を通す。
            graphPage.EnterNodeSelection(node);
            return new { success = true, selected = id };
        }

        object SetNodeMuted(uint id, bool muted)
        {
            if (!TryGetNode(id, out var node)) return UnknownNode(id);

            node.IsMuted.Value = muted;
            return new { success = true, id, muted };
        }

        object InvokeNodeAction(uint id)
        {
            if (!TryGetNode(id, out var node)) return UnknownNode(id);

            node.DoAction();
            return new { success = true, id };
        }

        // HUD は「繋がっていれば外す」トグルなので、繋がっている組に接続処理が届くことがない。
        // CLI は connect と disconnect を分けているぶんその保護がないが、既接続の判定も
        // GraphPage 側にあるので、両者で検証がずれることはない。
        object Connect(uint fromNode, int fromSlot, uint toNode, int toSlot)
        {
            if (!TryGetSlots(fromNode, fromSlot, toNode, toSlot, out var output, out var input, out var error)) return error;

            return graphPage.TryConnectSlots(output, input) switch
            {
                ConnectResult.Connected => new { success = true, fromNode, fromSlot, toNode, toSlot },
                ConnectResult.AlreadyConnected => Failure("already_connected", $"{fromNode}[{fromSlot}] -> {toNode}[{toSlot}] is already connected."),
                ConnectResult.Loop => Failure("loop_detected", $"Connecting {fromNode} -> {toNode} would create a loop."),
                ConnectResult.Incompatible => Failure("incompatible_slots", $"{output.Type} -> {input.Type} is not connectable."),
                ConnectResult.Failed => Failure("connect_failed", "EdgeConnector refused the connection."),
                _ => throw new ArgumentOutOfRangeException(),
            };
        }

        object Disconnect(uint fromNode, int fromSlot, uint toNode, int toSlot)
        {
            if (!TryGetSlots(fromNode, fromSlot, toNode, toSlot, out var output, out var input, out var error)) return error;

            if (!graphPage.DisconnectSlots(output, input))
                return Failure("no_such_edge", "There is no edge between those slots.");

            return new { success = true, fromNode, fromSlot, toNode, toSlot };
        }

        object SortGraph()
        {
            graphPage.Sort();
            return new { success = true, nodeCount = graphPage.Graph.NodeCount, edgeCount = graphPage.Graph.EdgeCount };
        }

        // ------------------------------------------------------ シーン / カメラ / VFX

        object LoadScene(string name)
        {
            if (!bgSceneManager.GetScenes().Contains(name))
                return Failure("unknown_scene", $"No scene named '{name}'.");

            bgSceneManager.Load(name);
            return new { success = true, scene = name };
        }

        object SetCamera(int index)
        {
            var cameras = cameraManager.GetCameraBehaviours();
            if (index < 0 || index >= cameras.Length)
                return Failure("index_out_of_range", $"Camera index must be 0..{cameras.Length - 1}.");

            cameras[index].IsActive.Value = true;
            return new { success = true, index, name = cameras[index].Name };
        }

        object ToggleVfx(string name)
        {
            var vfx = vfxManager.GetAllVfx().FirstOrDefault(v => v.Name == name);
            if (vfx == null) return Failure("unknown_vfx", $"No VFX named '{name}'.");

            vfx.ToggleActive();
            return new { success = true, name };
        }

        // ---------------------------------------------------------------- 補助

        System.Collections.Generic.IEnumerable<LayeredNode> Nodes() =>
            graphPage.Graph.Layers.SelectMany(layer => layer).OfType<LayeredNode>();

        bool TryGetNode(uint id, out Rector.UI.Graphs.Nodes.Node node)
        {
            if (graphPage.Graph.TryGetNode(new NodeId(id), out var layered))
            {
                node = layered.NodeView.Node;
                return true;
            }

            node = null;
            return false;
        }

        bool TryGetSlots(uint fromNode, int fromSlot, uint toNode, int toSlot,
            out OutputSlot output, out InputSlot input, out object error)
        {
            output = null;
            input = null;

            if (!TryGetNode(fromNode, out var from))
            {
                error = UnknownNode(fromNode);
                return false;
            }

            if (!TryGetNode(toNode, out var to))
            {
                error = UnknownNode(toNode);
                return false;
            }

            if (fromSlot < 0 || fromSlot >= from.OutputSlots.Length)
            {
                error = Failure("slot_out_of_range", $"Node {fromNode} has {from.OutputSlots.Length} output slot(s).");
                return false;
            }

            if (toSlot < 0 || toSlot >= to.InputSlots.Length)
            {
                error = Failure("slot_out_of_range", $"Node {toNode} has {to.InputSlots.Length} input slot(s).");
                return false;
            }

            output = from.OutputSlots[fromSlot];
            input = to.InputSlots[toSlot];
            error = null;
            return true;
        }

        object ToNodeDto(NodeId id) =>
            graphPage.Graph.TryGetNode(id, out var layered) ? ToNodeDto(layered) : null;

        static object ToNodeDto(LayeredNode layered)
        {
            var node = layered.NodeView.Node;
            return new
            {
                id = node.Id.Value,
                name = node.Name,
                category = node.Category.ToString(),
                layer = layered.Layer,
                column = layered.Column,
                // レイアウトをCLIから検証できるように、確定後の座標も返す
                x = layered.TargetPosition.x,
                y = layered.TargetPosition.y,
                width = layered.Width,
                muted = node.IsMuted.Value,
                selected = node.Selected.Value,
                inputs = node.InputSlots.Select(ToSlotDto).ToArray(),
                outputs = node.OutputSlots.Select(ToSlotDto).ToArray(),
            };
        }

        static object ToSlotDto(ISlot slot) => new
        {
            index = slot.Index,
            name = slot.Name,
            type = slot.Type.ToString(),
            connected = slot.ConnectedCount,
        };

        static object Failure(string code, string message) => new { success = false, error = code, message };

        static object UnknownNode(uint id) => Failure("unknown_node", $"No node with id {id}.");

        static object NotRunning() =>
            Failure("not_running", "Rector is not running. Enter play mode, or launch a development build.");

        static object Call(Func<CliClient, object> f) => Instance is { } c ? f(c) : NotRunning();
    }
}
