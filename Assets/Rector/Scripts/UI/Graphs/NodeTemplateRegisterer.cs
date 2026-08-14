using Rector.Audio;
using Rector.Cameras;
using Rector.Midi;
using Rector.NodeBehaviours;
using Rector.Osc;
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
        readonly SequenceModel sequenceModel;
        readonly AudioMixerModel audioMixerModel;
        readonly MidiModel midiModel;
        readonly OscModel oscModel;
        readonly CameraManager cameraManager;
        readonly HudModel hudModel;

        public NodeTemplateRegisterer(
            NodeTemplateRepository nodeTemplateRepository,
            NodeBehaviourProxyRepository proxyRepository,
            VfxManager vfxManager,
            BeatModel beatModel,
            SequenceModel sequenceModel,
            AudioMixerModel audioMixerModel,
            MidiModel midiModel,
            OscModel oscModel,
            CameraManager cameraManager,
            HudModel hudModel)
        {
            this.nodeTemplateRepository = nodeTemplateRepository;
            this.proxyRepository = proxyRepository;
            this.vfxManager = vfxManager;
            this.beatModel = beatModel;
            this.sequenceModel = sequenceModel;
            this.audioMixerModel = audioMixerModel;
            this.midiModel = midiModel;
            this.oscModel = oscModel;
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
                nodeTemplateRepository.Add(NodeTemplate.Behaviour(vfx.Guid, VfxNode.GetCategory(), vfx.Name, id => CreateNodeView(new VfxNode(id, vfx))));
            }

            foreach (var camera in cameraManager.GetCameraBehaviours())
            {
                nodeTemplateRepository.Add(NodeTemplate.Behaviour(camera.Guid, CameraNode.GetCategory(), camera.Name, id => CreateNodeView(new CameraNode(id, camera))));
            }

            nodeTemplateRepository.Add(NodeTemplate.Code<CameraBlendNode>(CameraBlendNode.GetCategory(), CameraBlendNode.NodeName, id => CreateNodeView(new CameraBlendNode(id, cameraManager))));

            nodeTemplateRepository.Add(NodeTemplate.Code<AudioThresholdNode>(AudioThresholdNode.GetCategory(), AudioThresholdNode.NodeName, id => CreateNodeView(new AudioThresholdNode(id, audioMixerModel))));
            nodeTemplateRepository.Add(NodeTemplate.Code<LevelNode>(LevelNode.GetCategory(), LevelNode.NodeName, id => CreateNodeView(new LevelNode(id, audioMixerModel))));
            nodeTemplateRepository.Add(NodeTemplate.Code<BeatNode>(BeatNode.GetCategory(), BeatNode.NodeName, id => CreateNodeView(new BeatNode(id, beatModel))));
            nodeTemplateRepository.Add(NodeTemplate.Code<SequenceNode>(SequenceNode.GetCategory(), SequenceNode.NodeName, id => CreateNodeView(new SequenceNode(id, sequenceModel))));
            nodeTemplateRepository.Add(NodeTemplate.Code<MidiNoteNode>(MidiNoteNode.GetCategory(), MidiNoteNode.NodeName, id => CreateNodeView(new MidiNoteNode(id, midiModel))));
            nodeTemplateRepository.Add(NodeTemplate.Code<MidiCcNode>(MidiCcNode.GetCategory(), MidiCcNode.NodeName, id => CreateNodeView(new MidiCcNode(id, midiModel))));
            nodeTemplateRepository.Add(NodeTemplate.Code<OscNode>(OscNode.GetCategory(), OscNode.NodeName, id => CreateNodeView(new OscNode(id, oscModel))));
            nodeTemplateRepository.Add(NodeTemplate.Code<UpdateNode>(UpdateNode.GetCategory(), UpdateNode.NodeName, id => CreateNodeView(new UpdateNode(id))));
            nodeTemplateRepository.Add(NodeTemplate.Code<TimeNode>(TimeNode.GetCategory(), TimeNode.NodeName, id => CreateNodeView(new TimeNode(id))));
            nodeTemplateRepository.Add(NodeTemplate.Code<ButtonNode>(ButtonNode.GetCategory(), ButtonNode.NodeName, id => CreateNodeView(new ButtonNode(id))));

            nodeTemplateRepository.Add(NodeTemplate.Code<Switch2Node>(Switch2Node.GetCategory(), Switch2Node.NodeName, id => CreateNodeView(new Switch2Node(id))));
            nodeTemplateRepository.Add(NodeTemplate.Code<Switch4Node>(Switch4Node.GetCategory(), Switch4Node.NodeName, id => CreateNodeView(new Switch4Node(id))));
            nodeTemplateRepository.Add(NodeTemplate.Code<Switch16Node>(Switch16Node.GetCategory(), Switch16Node.NodeName, id => CreateNodeView(new Switch16Node(id))));
            nodeTemplateRepository.Add(NodeTemplate.Code<Switch4By4Node>(Switch4By4Node.GetCategory(), Switch4By4Node.NodeName, id => CreateNodeView(new Switch4By4Node(id))));
            nodeTemplateRepository.Add(NodeTemplate.Code<Branch2Node>(Branch2Node.GetCategory(), Branch2Node.NodeName, id => CreateNodeView(new Branch2Node(id))));
            nodeTemplateRepository.Add(NodeTemplate.Code<Branch4Node>(Branch4Node.GetCategory(), Branch4Node.NodeName, id => CreateNodeView(new Branch4Node(id))));
            nodeTemplateRepository.Add(NodeTemplate.Code<Branch16Node>(Branch16Node.GetCategory(), Branch16Node.NodeName, id => CreateNodeView(new Branch16Node(id))));
            nodeTemplateRepository.Add(NodeTemplate.Code<EqualNode>(EqualNode.GetCategory(), EqualNode.NodeName, id => CreateNodeView(new EqualNode(id))));
            nodeTemplateRepository.Add(NodeTemplate.Code<AndNode>(AndNode.GetCategory(), AndNode.NodeName, id => CreateNodeView(new AndNode(id))));
            nodeTemplateRepository.Add(NodeTemplate.Code<OrNode>(OrNode.GetCategory(), OrNode.NodeName, id => CreateNodeView(new OrNode(id))));
            nodeTemplateRepository.Add(NodeTemplate.Code<GateNode>(GateNode.GetCategory(), GateNode.NodeName, id => CreateNodeView(new GateNode(id))));
            nodeTemplateRepository.Add(NodeTemplate.Code<NegateNode>(NegateNode.GetCategory(), NegateNode.NodeName, id => CreateNodeView(new NegateNode(id))));
            nodeTemplateRepository.Add(NodeTemplate.Code<WithNode>(WithNode.GetCategory(), WithNode.NodeName, id => CreateNodeView(new WithNode(id))));
            nodeTemplateRepository.Add(NodeTemplate.Code<LoopNode>(LoopNode.GetCategory(), LoopNode.NodeName, id => CreateNodeView(new LoopNode(id))));

            nodeTemplateRepository.Add(NodeTemplate.Code<MadNode>(MadNode.GetCategory(), MadNode.NodeName, id => CreateNodeView(new MadNode(id))));
            nodeTemplateRepository.Add(NodeTemplate.Code<FloatNode>(FloatNode.GetCategory(), FloatNode.NodeName, id => CreateNodeView(new FloatNode(id))));
            nodeTemplateRepository.Add(NodeTemplate.Code<Vector3Node>(Vector3Node.GetCategory(), Vector3Node.NodeName, id => CreateNodeView(new Vector3Node(id))));
            nodeTemplateRepository.Add(NodeTemplate.Code<SinNode>(SinNode.GetCategory(), SinNode.NodeName, id => CreateNodeView(new SinNode(id))));
            nodeTemplateRepository.Add(NodeTemplate.Code<CosNode>(CosNode.GetCategory(), CosNode.NodeName, id => CreateNodeView(new CosNode(id))));
            nodeTemplateRepository.Add(NodeTemplate.Code<MinNode>(MinNode.GetCategory(), MinNode.NodeName, id => CreateNodeView(new MinNode(id))));
            nodeTemplateRepository.Add(NodeTemplate.Code<MaxNode>(MaxNode.GetCategory(), MaxNode.NodeName, id => CreateNodeView(new MaxNode(id))));
            nodeTemplateRepository.Add(NodeTemplate.Code<FractNode>(FractNode.GetCategory(), FractNode.NodeName, id => CreateNodeView(new FractNode(id))));
            nodeTemplateRepository.Add(NodeTemplate.Code<StepNode>(StepNode.GetCategory(), StepNode.NodeName, id => CreateNodeView(new StepNode(id))));
            nodeTemplateRepository.Add(NodeTemplate.Code<CircleNode>(CircleNode.GetCategory(), CircleNode.NodeName, id => CreateNodeView(new CircleNode(id))));

            nodeTemplateRepository.Add(NodeTemplate.Code<HudStyleNode>(HudStyleNode.GetCategory(), HudStyleNode.NodeName, id => CreateNodeView(new HudStyleNode(id, hudModel))));

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
                case SequenceNode sequenceNode:
                    {
                        var ve = VisualElementFactory.Instance.CreateNode();
                        var nodeView = new SequenceNodeView(ve, sequenceNode);
                        return nodeView;
                    }
                case LoopNode loopNode:
                    {
                        var ve = VisualElementFactory.Instance.CreateNode();
                        var nodeView = new LoopNodeView(ve, loopNode);
                        return nodeView;
                    }
                case LearnableSourceNode learnableSourceNode:
                    {
                        var ve = VisualElementFactory.Instance.CreateNode();
                        var nodeView = new LearnableSourceNodeView(ve, learnableSourceNode);
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
