using Unity.Pipeline.Commands;

namespace Rector.Cli
{
    /// <summary>
    /// CliClient の CLI コマンド定義。
    /// 実処理は CliClient のインスタンスメソッド側に置き、ここは Instance への
    /// ディスパッチと引数の宣言だけに留める。
    /// </summary>
    public sealed partial class CliClient
    {
        [CliCommand("rector_state", "Current camera, background scene, graph size and selection.")]
        static object StateCommand() => Call(c => c.GetState());

        [CliCommand("rector_graph", "Full node graph: every node with its slots, plus every edge.")]
        static object GraphCommand() => Call(c => c.GetGraph());

        [CliCommand("rector_node_templates", "Node templates available to rector_create_node.")]
        static object NodeTemplatesCommand() => Call(c => c.GetNodeTemplates());

        [CliCommand("rector_vfx", "All VFX loaded by VfxManager.")]
        static object VfxCommand() => Call(c => c.GetVfx());

        [CliCommand("rector_cameras", "Camera behaviours, which one is active, and the blend time.")]
        static object CamerasCommand() => Call(c => c.GetCameras());

        [CliCommand("rector_scenes", "Background scenes available, and the current one.")]
        static object ScenesCommand() => Call(c => c.GetScenes());

        [CliCommand("rector_create_node", "Create a node from a template and add it to the graph.")]
        static object CreateNodeCommand(
            [CliArg("template", "Template name, as listed by rector_node_templates.", Required = true)] string template)
            => Call(c => c.CreateNode(template));

        [CliCommand("rector_remove_node", "Remove a node and the edges attached to it. Not undoable: requires confirm=true.")]
        static object RemoveNodeCommand(
            [CliArg("id", "Node id, as listed by rector_graph.", Required = true)] uint id,
            [CliArg("confirm", "Apply the removal. Without it the call is refused.")] bool confirm = false)
            => Call(c => c.RemoveNode(id, confirm));

        [CliCommand("rector_select_node", "Select a node, as if it had been selected in the HUD.")]
        static object SelectNodeCommand(
            [CliArg("id", "Node id.", Required = true)] uint id)
            => Call(c => c.SelectNode(id));

        [CliCommand("rector_set_node_muted", "Mute or unmute a node.")]
        static object SetNodeMutedCommand(
            [CliArg("id", "Node id.", Required = true)] uint id,
            [CliArg("muted", "true to mute, false to unmute.", Required = true)] bool muted)
            => Call(c => c.SetNodeMuted(id, muted));

        [CliCommand("rector_set_node_group", "Move a node to another group of the graph layout.")]
        static object SetNodeGroupCommand(
            [CliArg("id", "Node id.", Required = true)] uint id,
            [CliArg("group", "Group number as shown in the HUD (\"GROUP n\"), starting at 1. rector_state reports groupCount.", Required = true)] int group)
            => Call(c => c.SetNodeGroup(id, group));

        [CliCommand("rector_invoke_node", "Invoke a node's action (Node.DoAction).")]
        static object InvokeNodeCommand(
            [CliArg("id", "Node id.", Required = true)] uint id)
            => Call(c => c.InvokeNodeAction(id));

        [CliCommand("rector_connect", "Connect an output slot to an input slot. Refuses loops and incompatible types.")]
        static object ConnectCommand(
            [CliArg("from_node", "Node id owning the output slot.", Required = true)] uint fromNode,
            [CliArg("from_slot", "Output slot index.", Required = true)] int fromSlot,
            [CliArg("to_node", "Node id owning the input slot.", Required = true)] uint toNode,
            [CliArg("to_slot", "Input slot index.", Required = true)] int toSlot)
            => Call(c => c.Connect(fromNode, fromSlot, toNode, toSlot));

        [CliCommand("rector_disconnect", "Remove the edge between an output slot and an input slot.")]
        static object DisconnectCommand(
            [CliArg("from_node", "Node id owning the output slot.", Required = true)] uint fromNode,
            [CliArg("from_slot", "Output slot index.", Required = true)] int fromSlot,
            [CliArg("to_node", "Node id owning the input slot.", Required = true)] uint toNode,
            [CliArg("to_slot", "Input slot index.", Required = true)] int toSlot)
            => Call(c => c.Disconnect(fromNode, fromSlot, toNode, toSlot));

        [CliCommand("rector_sort_graph", "Re-run the layered graph layout.")]
        static object SortGraphCommand() => Call(c => c.SortGraph());

        [CliCommand("rector_load_scene", "Load a background scene by name.")]
        static object LoadSceneCommand(
            [CliArg("name", "Scene name, as listed by rector_scenes.", Required = true)] string name)
            => Call(c => c.LoadScene(name));

        [CliCommand("rector_set_camera", "Activate a camera behaviour by index.")]
        static object SetCameraCommand(
            [CliArg("index", "Camera index, as listed by rector_cameras.", Required = true)] int index)
            => Call(c => c.SetCamera(index));

        [CliCommand("rector_toggle_vfx", "Toggle a VFX on or off by name.")]
        static object ToggleVfxCommand(
            [CliArg("name", "VFX name, as listed by rector_vfx.", Required = true)] string name)
            => Call(c => c.ToggleVfx(name));
    }
}
