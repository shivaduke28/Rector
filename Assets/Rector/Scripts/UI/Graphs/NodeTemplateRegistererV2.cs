using Rector.Audio;
using Rector.Cameras;
using Rector.UI.Graphs.Nodes;
using Rector.UI.Hud;
using Rector.Vfx;

namespace Rector.UI.Graphs
{
    public sealed class NodeTemplateRegistererV2 : IInitializable
    {
        readonly NodeTemplateRepositoryV2 nodeTemplateRepository;
        readonly VfxManager vfxManager;
        readonly BeatModel beatModel;
        readonly AudioMixerModel audioMixerModel;
        readonly CameraManager cameraManager;
        readonly HudModel hudModel;

        public NodeTemplateRegistererV2(
            NodeTemplateRepositoryV2 nodeTemplateRepository,
            VfxManager vfxManager,
            BeatModel beatModel,
            AudioMixerModel audioMixerModel,
            CameraManager cameraManager,
            HudModel hudModel)
        {
            this.nodeTemplateRepository = nodeTemplateRepository;
            this.vfxManager = vfxManager;
            this.beatModel = beatModel;
            this.audioMixerModel = audioMixerModel;
            this.cameraManager = cameraManager;
            this.hudModel = hudModel;
        }

        void IInitializable.Initialize()
        {
            RegisterBuiltInNodes();
        }

        void RegisterBuiltInNodes()
        {
            foreach (var vfx in vfxManager.GetAllVfx())
            {
                nodeTemplateRepository.Add(NodeTemplateV2.Create(VfxNode.Category, vfx.Name, id => CreateNodeView(new VfxNode(id, vfx))));
            }

            foreach (var camera in cameraManager.GetCameraBehaviours())
            {
                nodeTemplateRepository.Add(NodeTemplateV2.Create(CameraNode.Category, camera.Name, id => CreateNodeView(new CameraNode(id, camera))));
            }

            nodeTemplateRepository.Add(NodeTemplateV2.Create(CameraBlendNode.Category, CameraBlendNode.NodeName, id => CreateNodeView(new CameraBlendNode(id, cameraManager))));

            nodeTemplateRepository.Add(NodeTemplateV2.Create(LevelNode.Category, LevelNode.NodeName, id => CreateNodeView(new LevelNode(id, audioMixerModel))));
            nodeTemplateRepository.Add(NodeTemplateV2.Create(AudioThresholdNode.Category, AudioThresholdNode.NodeName, id => CreateNodeView(new AudioThresholdNode(id, audioMixerModel))));
            nodeTemplateRepository.Add(NodeTemplateV2.Create(BeatNode.Category, BeatNode.NodeName, id => CreateNodeView(new BeatNode(id, beatModel))));
            nodeTemplateRepository.Add(NodeTemplateV2.Create(UpdateNode.Category, UpdateNode.NodeName, id => CreateNodeView(new UpdateNode(id))));
            nodeTemplateRepository.Add(NodeTemplateV2.Create(TimeNode.Category, TimeNode.NodeName, id => CreateNodeView(new TimeNode(id))));
            nodeTemplateRepository.Add(NodeTemplateV2.Create(ButtonNode.Category, ButtonNode.NodeName, id => CreateNodeView(new ButtonNode(id))));

            nodeTemplateRepository.Add(NodeTemplateV2.Create(Switch2Node.Category, Switch2Node.NodeName, id => CreateNodeView(new Switch2Node(id))));
            nodeTemplateRepository.Add(NodeTemplateV2.Create(Switch4Node.Category, Switch4Node.NodeName, id => CreateNodeView(new Switch4Node(id))));
            nodeTemplateRepository.Add(NodeTemplateV2.Create(Switch16Node.Category, Switch16Node.NodeName, id => CreateNodeView(new Switch16Node(id))));
            nodeTemplateRepository.Add(NodeTemplateV2.Create(Switch4By4Node.Category, Switch4By4Node.NodeName, id => CreateNodeView(new Switch4By4Node(id))));
            nodeTemplateRepository.Add(NodeTemplateV2.Create(AndNode.Category, AndNode.NodeName, id => CreateNodeView(new AndNode(id))));
            nodeTemplateRepository.Add(NodeTemplateV2.Create(OrNode.Category, OrNode.NodeName, id => CreateNodeView(new OrNode(id))));
            nodeTemplateRepository.Add(NodeTemplateV2.Create(GateNode.Category, GateNode.NodeName, id => CreateNodeView(new GateNode(id))));
            nodeTemplateRepository.Add(NodeTemplateV2.Create(NegateNode.Category, NegateNode.NodeName, id => CreateNodeView(new NegateNode(id))));
            nodeTemplateRepository.Add(NodeTemplateV2.Create(WithNode.Category, WithNode.NodeName, id => CreateNodeView(new WithNode(id))));

            nodeTemplateRepository.Add(NodeTemplateV2.Create(MadNode.Category, MadNode.NodeName, id => CreateNodeView(new MadNode(id))));
            nodeTemplateRepository.Add(NodeTemplateV2.Create(FloatNode.Category, FloatNode.NodeName, id => CreateNodeView(new FloatNode(id))));
            nodeTemplateRepository.Add(NodeTemplateV2.Create(Vector3Node.Category, Vector3Node.NodeName, id => CreateNodeView(new Vector3Node(id))));
            nodeTemplateRepository.Add(NodeTemplateV2.Create(SinNode.Category, SinNode.NodeName, id => CreateNodeView(new SinNode(id))));
            nodeTemplateRepository.Add(NodeTemplateV2.Create(CosNode.Category, CosNode.NodeName, id => CreateNodeView(new CosNode(id))));
            nodeTemplateRepository.Add(NodeTemplateV2.Create(MinNode.Category, MinNode.NodeName, id => CreateNodeView(new MinNode(id))));
            nodeTemplateRepository.Add(NodeTemplateV2.Create(MaxNode.Category, MaxNode.NodeName, id => CreateNodeView(new MaxNode(id))));
            nodeTemplateRepository.Add(NodeTemplateV2.Create(ModNode.Category, ModNode.NodeName, id => CreateNodeView(new ModNode(id))));
            nodeTemplateRepository.Add(NodeTemplateV2.Create(StepNode.Category, StepNode.NodeName, id => CreateNodeView(new StepNode(id))));
            nodeTemplateRepository.Add(NodeTemplateV2.Create(CircleNode.Category, CircleNode.NodeName, id => CreateNodeView(new CircleNode(id))));

            nodeTemplateRepository.Add(NodeTemplateV2.Create(HudStyleNode.Category, HudStyleNode.NodeName, id => CreateNodeView(new HudStyleNode(id, hudModel))));

            /* Add your custom node here  */
        }

        NodeView CreateNodeView(Node node)
        {
            switch (node)
            {
                case BeatNode beatNode:
                {
                    var ve = VisualElementFactory.Instance.CreateNode();
                    var nodeView = new BeatNodeView(ve, beatNode);
                    return nodeView;
                }
                /* You can add custom node view here */
                default:
                {
                    var ve = VisualElementFactory.Instance.CreateNode();
                    var nodeView = new NodeView(ve, node);
                    return nodeView;
                }
            }
        }
    }
}
