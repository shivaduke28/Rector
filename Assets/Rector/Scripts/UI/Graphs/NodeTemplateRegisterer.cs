using Rector.Audio;
using Rector.Cameras;
using Rector.UI.Hud;
using Rector.UI.Nodes;
using Rector.Vfx;

namespace Rector.UI.Graphs
{
    public sealed class NodeTemplateRegisterer : IInitializable
    {
        readonly NodeTemplateRepository nodeTemplateRepository;
        readonly VfxManager vfxManager;
        readonly BeatModel beatModel;
        readonly AudioMixerModel audioMixerModel;
        readonly CameraManager cameraManager;
        readonly HudModel hudModel;

        public NodeTemplateRegisterer(
            NodeTemplateRepository nodeTemplateRepository,
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
                nodeTemplateRepository.Add(NodeTemplate.Create(NodeCategory.Vfx, vfx.Name, id => new VfxNode(id, vfx)));
            }

            foreach (var camera in cameraManager.GetCameraBehaviours())
            {
                nodeTemplateRepository.Add(NodeTemplate.Create(NodeCategory.Camera, camera.Name, id => new CameraNode(id, camera)));
            }

            nodeTemplateRepository.Add(NodeTemplate.Create(NodeCategory.Camera, CameraBlendNode.NodeName, id => new CameraBlendNode(id, cameraManager)));

            nodeTemplateRepository.Add(NodeTemplate.Create(NodeCategory.Event, LevelNode.NodeName, id => new LevelNode(id, audioMixerModel)));
            nodeTemplateRepository.Add(NodeTemplate.Create(NodeCategory.Event, AudioThresholdNode.NodeName, id => new AudioThresholdNode(id, audioMixerModel)));
            nodeTemplateRepository.Add(NodeTemplate.Create(NodeCategory.Event, BeatNode.NodeName, id => new BeatNode(id, beatModel)));
            nodeTemplateRepository.Add(NodeTemplate.Create(NodeCategory.Event, UpdateNode.NodeName, id => new UpdateNode(id)));
            nodeTemplateRepository.Add(NodeTemplate.Create(NodeCategory.Event, TimeNode.NodeName, id => new TimeNode(id)));
            nodeTemplateRepository.Add(NodeTemplate.Create(NodeCategory.Event, ButtonNode.NodeName, id => new ButtonNode(id)));

            nodeTemplateRepository.Add(NodeTemplate.Create(NodeCategory.Operator, Switch2Node.NodeName, id => new Switch2Node(id)));
            nodeTemplateRepository.Add(NodeTemplate.Create(NodeCategory.Operator, Switch4Node.NodeName, id => new Switch4Node(id)));
            nodeTemplateRepository.Add(NodeTemplate.Create(NodeCategory.Operator, Switch16Node.NodeName, id => new Switch16Node(id)));
            nodeTemplateRepository.Add(NodeTemplate.Create(NodeCategory.Operator, Switch4By4Node.NodeName, id => new Switch4By4Node(id)));
            nodeTemplateRepository.Add(NodeTemplate.Create(NodeCategory.Operator, AndNode.NodeName, id => new AndNode(id)));
            nodeTemplateRepository.Add(NodeTemplate.Create(NodeCategory.Operator, OrNode.NodeName, id => new OrNode(id)));
            nodeTemplateRepository.Add(NodeTemplate.Create(NodeCategory.Operator, GateNode.NodeName, id => new GateNode(id)));
            nodeTemplateRepository.Add(NodeTemplate.Create(NodeCategory.Operator, NegateNode.NodeName, id => new NegateNode(id)));
            nodeTemplateRepository.Add(NodeTemplate.Create(NodeCategory.Operator, WithNode.NodeName, id => new WithNode(id)));

            nodeTemplateRepository.Add(NodeTemplate.Create(NodeCategory.Math, MadNode.NodeName, id => new MadNode(id)));
            nodeTemplateRepository.Add(NodeTemplate.Create(NodeCategory.Math, FloatNode.NodeName, id => new FloatNode(id)));
            nodeTemplateRepository.Add(NodeTemplate.Create(NodeCategory.Math, Vector3Node.NodeName, id => new Vector3Node(id)));
            nodeTemplateRepository.Add(NodeTemplate.Create(NodeCategory.Math, SinNode.NodeName, id => new SinNode(id)));
            nodeTemplateRepository.Add(NodeTemplate.Create(NodeCategory.Math, CosNode.NodeName, id => new CosNode(id)));
            nodeTemplateRepository.Add(NodeTemplate.Create(NodeCategory.Math, MinNode.NodeName, id => new MinNode(id)));
            nodeTemplateRepository.Add(NodeTemplate.Create(NodeCategory.Math, MaxNode.NodeName, id => new MaxNode(id)));
            nodeTemplateRepository.Add(NodeTemplate.Create(NodeCategory.Math, ModNode.NodeName, id => new ModNode(id)));
            nodeTemplateRepository.Add(NodeTemplate.Create(NodeCategory.Math, StepNode.NodeName, id => new StepNode(id)));
            nodeTemplateRepository.Add(NodeTemplate.Create(NodeCategory.Math, CircleNode.NodeName, id => new CircleNode(id)));

            nodeTemplateRepository.Add(NodeTemplate.Create(NodeCategory.System, HudStyleNode.NodeName, id => new HudStyleNode(id, hudModel)));
        }
    }
}
