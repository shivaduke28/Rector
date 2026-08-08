using System;
using System.Collections.Generic;
using R3;
using Rector.UI.Graphs.Slots;
using UnityEngine;
using UnityEngine.UIElements;

namespace Rector.UI.Graphs.Nodes
{
    public class NodeView : IDisposable
    {
        protected readonly VisualElement Root;
        protected readonly Label NameLabel;
        protected readonly VisualElement InputSlotList;
        protected readonly VisualElement OutputSlotList;

        public Node Node { get; }
        public float Width => Root.resolvedStyle.width;
        public float Height => Root.resolvedStyle.height;
        protected const string NodeSelectedClassName = "rector-node--selected";
        protected const string NodeTargetClassName = "rector-node--target";
        protected const string NodeGrabbedClassName = "rector-node--grabbed";
        const string TargetOutlineClassName = "rector-node-target-outline";
        const string GrabArrowClassName = "rector-node-grab-arrow";
        const string GrabArrowLeftClassName = GrabArrowClassName + "-left";
        const string GrabArrowRightClassName = GrabArrowClassName + "-right";
        const long GrabBlinkIntervalMs = 700;
        public List<InputSlotView> InputSlotViews { get; }
        public List<OutputSlotView> OutputSlotViews { get; }

        protected readonly CompositeDisposable Disposables = new();

        readonly Label leftGrabArrow;
        readonly Label rightGrabArrow;
        IVisualElementScheduledItem grabBlinkItem;
        bool grabBlinkVisible;

        // アニメーション中は補間中の現在値を返す。目標値が要る場合は TargetPosition を使う。
        public Vector2 Position
        {
            get => Root.resolvedStyle.translate;
            set
            {
                TargetPosition = value;
                Root.style.translate = value;
            }
        }

        public Vector2 TargetPosition { get; private set; }

        public NodeView(VisualElement templateContainer, Node node)
        {
            Root = templateContainer.Q<VisualElement>("node");
            NameLabel = Root.Q<Label>("name-label");
            InputSlotList = Root.Q<VisualElement>("input-slot-list");
            OutputSlotList = Root.Q<VisualElement>("output-slot-list");

            Root.Q<VisualElement>("icon").style.backgroundImage = new StyleBackground(GetCategoryIcon(node.Category));

            Node = node;
            NameLabel.text = node.Name;
            // ターゲット表示は色ではなく一回り大きいアウトライン(表示切替はUSSの--targetで行う)
            var targetOutline = new VisualElement { pickingMode = PickingMode.Ignore };
            targetOutline.AddToClassList(TargetOutlineClassName);
            Root.Add(targetOutline);

            leftGrabArrow = CreateGrabArrow("◀", GrabArrowLeftClassName);
            rightGrabArrow = CreateGrabArrow("▶", GrabArrowRightClassName);
            Root.Add(leftGrabArrow);
            Root.Add(rightGrabArrow);

            node.Selected.Subscribe(x => Root.EnableInClassList(NodeSelectedClassName, x)).AddTo(Disposables);
            node.IsTarget.Subscribe(x => Root.EnableInClassList(NodeTargetClassName, x)).AddTo(Disposables);
            node.IsGrabbed.Subscribe(SetGrabbed).AddTo(Disposables);
            InputSlotViews = new List<InputSlotView>(node.InputSlots.Length);
            foreach (var slot in node.InputSlots)
            {
                var slotView = new InputSlotView(VisualElementFactory.Instance.CreateInputSlot());
                slotView.Bind(slot).AddTo(Disposables);
                slotView.AddTo(InputSlotList);
                InputSlotViews.Add(slotView);
            }

            OutputSlotViews = new List<OutputSlotView>(node.OutputSlots.Length);
            foreach (var slot in node.OutputSlots)
            {
                var slotView = new OutputSlotView(VisualElementFactory.Instance.CreateOutputSlot());
                slotView.Bind(slot).AddTo(Disposables);
                slotView.AddTo(OutputSlotList);
                OutputSlotViews.Add(slotView);
            }

            node.IsMuted.Subscribe(x => Root.EnableInClassList("rector-node--muted", x)).AddTo(Disposables);
        }

        static Label CreateGrabArrow(string text, string sideClassName)
        {
            var label = new Label(text) { pickingMode = PickingMode.Ignore };
            label.AddToClassList(GrabArrowClassName);
            label.AddToClassList(sideClassName);
            return label;
        }

        void SetGrabbed(bool grabbed)
        {
            Root.EnableInClassList(NodeGrabbedClassName, grabbed);
            if (grabbed)
            {
                // 点滅はUSSのopacityトランジションと組み合わせて柔らかく明滅させる。
                // スケジューラの初回tickは位相がずれて即発火することがあるので、
                // falseから始めて最初のtickが「表示」側に倒れるようにする
                grabBlinkVisible = false;
                SetGrabArrowOpacity(1f);
                grabBlinkItem ??= Root.schedule.Execute(BlinkGrabArrows).Every(GrabBlinkIntervalMs);
                grabBlinkItem.Resume();
            }
            else
            {
                grabBlinkItem?.Pause();
            }
        }

        void BlinkGrabArrows()
        {
            grabBlinkVisible = !grabBlinkVisible;
            SetGrabArrowOpacity(grabBlinkVisible ? 1f : 0.15f);
        }

        void SetGrabArrowOpacity(float value)
        {
            leftGrabArrow.style.opacity = value;
            rightGrabArrow.style.opacity = value;
        }

        public void Dispose()
        {
            // 現状はRemoveFrom→Disposeの順で呼ばれてパネルから外れた時点で止まるが、
            // 呼び順に依存しないよう明示的に止めておく
            grabBlinkItem?.Pause();
            Disposables.Dispose();
        }
        public void AddTo(VisualElement parent) => parent.Add(Root);
        public void RemoveFrom(VisualElement parent) => parent.Remove(Root);

        static Texture2D GetCategoryIcon(NodeCategory category)
        {
            return category switch
            {
                NodeCategory.Vfx => VisualElementFactory.Instance.Icons.vfx,
                NodeCategory.Camera => VisualElementFactory.Instance.Icons.camera,
                NodeCategory.Event => VisualElementFactory.Instance.Icons.@event,
                NodeCategory.Operator => VisualElementFactory.Instance.Icons.@operator,
                NodeCategory.Math => VisualElementFactory.Instance.Icons.math,
                NodeCategory.Scene => VisualElementFactory.Instance.Icons.scene,
                NodeCategory.System => VisualElementFactory.Instance.Icons.system,
                NodeCategory.Input => VisualElementFactory.Instance.Icons.input,
                _ => null
            };
        }
    }
}
