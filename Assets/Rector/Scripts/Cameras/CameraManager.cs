using System;
using R3;
using Rector.NodeBehaviours;
using Unity.Cinemachine;

namespace Rector.Cameras
{
    public enum CameraBlend
    {
        Cut = 0,
        EaseInOut,
        EaseIn,
        EaseOut,
        HardIn,
        HardOut,
        Linear,
    }

    public sealed class CameraManager : IInitializable, IDisposable
    {
        readonly CinemachineBrain brain;
        readonly CameraNodeBehaviour[] cameraBehaviours;
        readonly CompositeDisposable disposables = new();
        readonly ReactiveProperty<string> currentCamera = new("");
        public CameraNodeBehaviour[] GetCameraBehaviours() => cameraBehaviours;
        public ReadOnlyReactiveProperty<string> CurrentCamera => currentCamera;

        readonly ReactiveProperty<CameraBlend> blendStyle = new(CameraBlend.Cut);
        public readonly BoolInput[] BlendInputs;
        public readonly ReactiveProperty<float> BlendTime = new(1f);

        public CameraManager(CinemachineBrain brain, CameraNodeBehaviour[] cameraBehaviours)
        {
            this.brain = brain;
            this.cameraBehaviours = cameraBehaviours;

            BlendInputs = new BoolInput[Enum.GetValues(typeof(CameraBlend)).Length];
            for (var i = 0; i < BlendInputs.Length; i++)
            {
                var blend = (CameraBlend)i;
                BlendInputs[i] = new BoolInput(blend.ToString(), false);
            }
        }

        public void Initialize()
        {
            foreach (var camera in cameraBehaviours)
            {
                camera.IsActive.Where(x => x).Subscribe(camera, (_, c) =>
                {
                    // RectorLogger.ActiveCamera(c.Name);
                    currentCamera.Value = c.Name;
                    DisableOthers(c);
                }).AddTo(disposables);
            }

            blendStyle.CombineLatest(BlendTime, (style, time) => (style, time)).Subscribe(tuple =>
            {
                var (style, time) = tuple;
                brain.DefaultBlend = style switch
                {
                    CameraBlend.Cut => new CinemachineBlendDefinition(CinemachineBlendDefinition.Styles.Cut, 0),
                    CameraBlend.EaseInOut => new CinemachineBlendDefinition(CinemachineBlendDefinition.Styles.EaseInOut, time),
                    CameraBlend.EaseIn => new CinemachineBlendDefinition(CinemachineBlendDefinition.Styles.EaseIn, time),
                    CameraBlend.EaseOut => new CinemachineBlendDefinition(CinemachineBlendDefinition.Styles.EaseOut, time),
                    CameraBlend.HardIn => new CinemachineBlendDefinition(CinemachineBlendDefinition.Styles.HardIn, time),
                    CameraBlend.HardOut => new CinemachineBlendDefinition(CinemachineBlendDefinition.Styles.HardOut, time),
                    CameraBlend.Linear => new CinemachineBlendDefinition(CinemachineBlendDefinition.Styles.Linear, time),
                    _ => throw new ArgumentOutOfRangeException(nameof(style), style, null)
                };
            }).AddTo(disposables);

            for(var i = 0; i < BlendInputs.Length; i++)
            {
                var index = i;
                BlendInputs[i].Value.Where(x => x).Subscribe(x =>
                {
                    blendStyle.Value = (CameraBlend)index;
                }).AddTo(disposables);
            }

            blendStyle.Subscribe(style =>
            {
                for (var i = 0; i < BlendInputs.Length; i++)
                {
                    BlendInputs[i].Value.Value = i == (int)style;
                }
            }).AddTo(disposables);
        }

        void DisableOthers(CameraNodeBehaviour cameraNode)
        {
            foreach (var other in cameraBehaviours)
            {
                other.IsActive.Value = other == cameraNode;
            }
        }

        public void Dispose()
        {
            disposables.Dispose();
        }
    }
}
