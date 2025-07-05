using Rector.Audio;
using Rector.Cameras;
using Rector.NodeBehaviours;
using Rector.UI.Graphs.Nodes;
using Rector.UI.Hud;
using Rector.Vfx;

namespace Rector.UI.Graphs
{
    public sealed class NodeTemplateRegisterer : IInitializable
    {
        readonly NodeTemplateRepository nodeTemplateRepository;
        readonly NodeBehaviourProxyRepository proxyRepository;
        readonly VfxManager vfxManager;
        readonly BeatModel beatModel;
        readonly AudioMixerModel audioMixerModel;
        readonly CameraManager cameraManager;
        readonly HudModel hudModel;

        public NodeTemplateRegisterer(
            NodeTemplateRepository nodeTemplateRepository,
            NodeBehaviourProxyRepository proxyRepository,
            VfxManager vfxManager,
            BeatModel beatModel,
            AudioMixerModel audioMixerModel,
            CameraManager cameraManager,
            HudModel hudModel)
        {
            this.nodeTemplateRepository = nodeTemplateRepository;
            this.proxyRepository = proxyRepository;
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
                nodeTemplateRepository.Add(NodeTemplate.Create(VfxNode.GetCategory(), vfx.Name, id => CreateNodeView(new VfxNode(id, vfx))));
            }

            foreach (var camera in cameraManager.GetCameraBehaviours())
            {
                nodeTemplateRepository.Add(NodeTemplate.Create(CameraNode.GetCategory(), camera.Name, id => CreateNodeView(new CameraNode(id, camera))));
            }

            nodeTemplateRepository.Add(NodeTemplate.Create(CameraBlendNode.GetCategory(), CameraBlendNode.NodeName, id => CreateNodeView(new CameraBlendNode(id, cameraManager))));

            nodeTemplateRepository.Add(NodeTemplate.Create(AudioThresholdNode.GetCategory(), AudioThresholdNode.NodeName, id => CreateNodeView(new AudioThresholdNode(id, audioMixerModel))));
            nodeTemplateRepository.Add(NodeTemplate.Create(LevelNode.GetCategory(), LevelNode.NodeName, id => CreateNodeView(new LevelNode(id, audioMixerModel))));
            nodeTemplateRepository.Add(NodeTemplate.Create(BeatNode.GetCategory(), BeatNode.NodeName, id => CreateNodeView(new BeatNode(id, beatModel))));
            nodeTemplateRepository.Add(NodeTemplate.Create(UpdateNode.GetCategory(), UpdateNode.NodeName, id => CreateNodeView(new UpdateNode(id))));
            nodeTemplateRepository.Add(NodeTemplate.Create(TimeNode.GetCategory(), TimeNode.NodeName, id => CreateNodeView(new TimeNode(id))));
            nodeTemplateRepository.Add(NodeTemplate.Create(ButtonNode.GetCategory(), ButtonNode.NodeName, id => CreateNodeView(new ButtonNode(id))));

            nodeTemplateRepository.Add(NodeTemplate.Create(Switch2Node.GetCategory(), Switch2Node.NodeName, id => CreateNodeView(new Switch2Node(id))));
            nodeTemplateRepository.Add(NodeTemplate.Create(Switch4Node.GetCategory(), Switch4Node.NodeName, id => CreateNodeView(new Switch4Node(id))));
            nodeTemplateRepository.Add(NodeTemplate.Create(Switch16Node.GetCategory(), Switch16Node.NodeName, id => CreateNodeView(new Switch16Node(id))));
            nodeTemplateRepository.Add(NodeTemplate.Create(Switch4By4Node.GetCategory(), Switch4By4Node.NodeName, id => CreateNodeView(new Switch4By4Node(id))));
            nodeTemplateRepository.Add(NodeTemplate.Create(AndNode.GetCategory(), AndNode.NodeName, id => CreateNodeView(new AndNode(id))));
            nodeTemplateRepository.Add(NodeTemplate.Create(OrNode.GetCategory(), OrNode.NodeName, id => CreateNodeView(new OrNode(id))));
            nodeTemplateRepository.Add(NodeTemplate.Create(GateNode.GetCategory(), GateNode.NodeName, id => CreateNodeView(new GateNode(id))));
            nodeTemplateRepository.Add(NodeTemplate.Create(NegateNode.GetCategory(), NegateNode.NodeName, id => CreateNodeView(new NegateNode(id))));
            nodeTemplateRepository.Add(NodeTemplate.Create(WithNode.GetCategory(), WithNode.NodeName, id => CreateNodeView(new WithNode(id))));

            nodeTemplateRepository.Add(NodeTemplate.Create(MadNode.GetCategory(), MadNode.NodeName, id => CreateNodeView(new MadNode(id))));
            nodeTemplateRepository.Add(NodeTemplate.Create(FloatNode.GetCategory(), FloatNode.NodeName, id => CreateNodeView(new FloatNode(id))));
            nodeTemplateRepository.Add(NodeTemplate.Create(Vector3Node.GetCategory(), Vector3Node.NodeName, id => CreateNodeView(new Vector3Node(id))));
            nodeTemplateRepository.Add(NodeTemplate.Create(SinNode.GetCategory(), SinNode.NodeName, id => CreateNodeView(new SinNode(id))));
            nodeTemplateRepository.Add(NodeTemplate.Create(CosNode.GetCategory(), CosNode.NodeName, id => CreateNodeView(new CosNode(id))));
            nodeTemplateRepository.Add(NodeTemplate.Create(MinNode.GetCategory(), MinNode.NodeName, id => CreateNodeView(new MinNode(id))));
            nodeTemplateRepository.Add(NodeTemplate.Create(MaxNode.GetCategory(), MaxNode.NodeName, id => CreateNodeView(new MaxNode(id))));
            nodeTemplateRepository.Add(NodeTemplate.Create(FractNode.GetCategory(), FractNode.NodeName, id => CreateNodeView(new FractNode(id))));
            nodeTemplateRepository.Add(NodeTemplate.Create(StepNode.GetCategory(), StepNode.NodeName, id => CreateNodeView(new StepNode(id))));
            nodeTemplateRepository.Add(NodeTemplate.Create(CircleNode.GetCategory(), CircleNode.NodeName, id => CreateNodeView(new CircleNode(id))));

            nodeTemplateRepository.Add(NodeTemplate.Create(HudStyleNode.GetCategory(), HudStyleNode.NodeName, id => CreateNodeView(new HudStyleNode(id, hudModel))));

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
