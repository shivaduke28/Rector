using System;
using System.Linq;
using Cysharp.Threading.Tasks;
using R3;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Rector.UI.GraphPages
{
    public enum HoldState
    {
        Start,
        Cancel,
        Perform,
    }

    /// <summary>グループ移動の1回分。Directionは-1か1。</summary>
    public readonly struct GroupMove
    {
        public readonly int Direction;

        /// <summary>Lock(L2/Tab)も押していたら、選択ノードの子孫も一緒に動かす。</summary>
        public readonly bool WithDescendants;

        public GroupMove(int direction, bool withDescendants)
        {
            Direction = direction;
            WithDescendants = withDescendants;
        }
    }

    public sealed class GraphInputAction : RectorInput.IGraphActions, IInitializable, IDisposable
    {
        readonly RectorInput rectorInput;
        readonly Subject<Unit> submit = new();
        readonly Subject<Unit> cancel = new();
        readonly Subject<Unit> action = new();
        readonly Subject<Unit> addNode = new();
        readonly Subject<HoldState> removeEdge = new();
        readonly Subject<HoldState> removeNode = new();
        readonly Subject<Unit> mute = new();
        readonly Subject<Unit> openNodeParameter = new();
        readonly Subject<Unit> closeNodeParameter = new();
        readonly Subject<Unit> openSystem = new();
        readonly Subject<Unit> openScene = new();
        readonly Subject<Unit> resetTransform = new();
        readonly Subject<GroupMove> moveNodeToGroup = new();

        readonly NavigateInputThrottle navigateInputThrottle = new();

        public Observable<Unit> Submit => submit;
        public Observable<Unit> Cancel => cancel;
        public Observable<Unit> Action => action;
        public Observable<Unit> AddNode => addNode;
        public Observable<HoldState> RemoveEdge => removeEdge;
        public Observable<HoldState> RemoveNode => removeNode;
        public Observable<Unit> Mute => mute;
        public ReadOnlyReactiveProperty<bool> GrabModifierHeld => grabModifierHeld;
        public ReadOnlyReactiveProperty<bool> LockHeld => lockHeld;
        public Observable<Unit> OpenNodeParameter => openNodeParameter;
        public Observable<Unit> CloseNodeParameter => closeNodeParameter;
        public Observable<Unit> OpenSystem => openSystem;
        public Observable<Unit> OpenScene => openScene;
        public Observable<Unit> ResetTransform => resetTransform;
        public Observable<Vector2> Navigate => navigateInputThrottle.Navigate;
        public Observable<GroupMove> MoveNodeToGroup => moveNodeToGroup;

        public Vector2 Translate { get; private set; }
        public float Zoom { get; private set; }
        public bool IsNodeParameterOpen => rectorInput.Graph.OpenNodeParameter.IsPressed();

        const float DirectionThreshold = 0.5f;

        enum NavigateDirection
        {
            None,
            Up,
            Down,
            Left,
            Right,
        }

        // ガイドバー等が購読できるようReactivePropertyで持つ
        readonly ReactiveProperty<bool> grabModifierHeld = new(false);

        // Grab中の十字キーが「子孫も一緒に」になるかを発火時に読む
        readonly ReactiveProperty<bool> lockHeld = new(false);

        /// <summary>
        /// 操作ガイドを光らせるための押下状態。位置ごとに1つ持つ。
        /// </summary>
        /// <remarks>
        /// phaseの意味がinteractionで変わるので、押下の取り方は位置ごとに選んでいる。
        /// interaction無しとHoldは started(押す)/canceled(離す) が揃うが、
        /// **Tapは離したときperformedで終わり canceled が来ない**。
        /// Tapが付いた Cancel / AddNode は同じ物理ボタンにHoldのアクション
        /// (RemoveEdge / RemoveNode)が同居していてバインディングも一致しているので、
        /// そちらを押下の代表として見る。バインディングを分けるときはここも直すこと。
        /// </remarks>
        readonly ReactiveProperty<bool>[] pressed =
            Enumerable.Range(0, Enum.GetValues(typeof(GuideInput)).Length)
                .Select(_ => new ReactiveProperty<bool>(false))
                .ToArray();

        public ReadOnlyReactiveProperty<bool> Pressed(GuideInput input) => pressed[(int)input];

        void SetPressed(GuideInput input, bool value) => pressed[(int)input].Value = value;
        NavigateDirection chordNavigateDirection;
        bool chordNeutralRequired;

        bool removeNodeHolding;
        int removeNodeHoldId;
        bool removeEdgeHolding;
        int removeEdgeHoldId;

        public GraphInputAction(RectorInput rectorInput)
        {
            this.rectorInput = rectorInput;
        }

        public void Enable()
        {
            rectorInput.Graph.Enable();
        }

        public void Disable()
        {
            Translate = Vector2.zero;
            Zoom = 0f;
            rectorInput.Graph.Disable();
        }

        public void OnNavigate(InputAction.CallbackContext context)
        {
            if (context.performed)
            {
                HandleNavigate(context.ReadValue<Vector2>());
            }
            else if (context.canceled)
            {
                HandleNavigate(Vector2.zero);
            }
        }

        void HandleNavigate(Vector2 value)
        {
            if (grabModifierHeld.Value)
            {
                HandleChordNavigate(value);
                return;
            }

            navigateInputThrottle.SetInput(value);
        }

        /// <remarks>
        /// GrabModifier(R2/Option)を押している間は十字キーを別コマンドとして扱う。
        /// 左右は選択ノードのグループ移動。Lock(L2/Tab)も押していれば子孫ごと動かす。
        /// 離散的な操作なのでリピートさせず、方向が新しく確定した瞬間だけ発火させる。
        /// </remarks>
        void HandleChordNavigate(Vector2 value)
        {
            // 斜めは判定保留(前の方向を維持)。Noneに落とすと、右→右下→右と転がっただけで
            // 「倒し直した」ことになりグループ移動が二重発火する。
            var direction = ToNavigateDirection(value);
            if (direction is not { } d) return;

            // 斜めに倒したまま修飾キーを押した場合は、一度中立を観測するまで発火させない。
            // ここを通すと、押し込んだ指が縦横に転がっただけで「新しい方向」として発火してしまう。
            if (chordNeutralRequired)
            {
                if (d != NavigateDirection.None) return;
                chordNeutralRequired = false;
                chordNavigateDirection = NavigateDirection.None;
                return;
            }

            if (d == chordNavigateDirection) return;

            // 上下も含めて方向は常に記録する。消すと右→上→右の転がりで右が再発火しなくなる。
            chordNavigateDirection = d;
            switch (d)
            {
                case NavigateDirection.Left:
                    moveNodeToGroup.OnNext(new GroupMove(-1, lockHeld.Value));
                    break;
                case NavigateDirection.Right:
                    moveNodeToGroup.OnNext(new GroupMove(1, lockHeld.Value));
                    break;
            }
        }

        /// <summary>優勢軸の方向に丸める。中立はNone、斜めと倒しきっていない入力はnull(判定不能)。</summary>
        static NavigateDirection? ToNavigateDirection(Vector2 value)
        {
            if (value.sqrMagnitude == 0f) return NavigateDirection.None;

            var x = Mathf.Abs(value.x);
            var y = Mathf.Abs(value.y);
            if (x == y || Mathf.Max(x, y) < DirectionThreshold) return null;
            if (x > y)
            {
                return value.x > 0 ? NavigateDirection.Right : NavigateDirection.Left;
            }

            return value.y > 0 ? NavigateDirection.Up : NavigateDirection.Down;
        }

        public void OnSubmit(InputAction.CallbackContext context)
        {
            UpdatePressed(GuideInput.FaceBottom, context);
            if (context.performed)
            {
                submit.OnNext(Unit.Default);
            }
        }

        // Cancel / AddNode はTapなので押下の追跡には使わない(pressedのremarks参照)。
        // 同じボタンの RemoveEdge / RemoveNode が代わりに光らせる。
        public void OnCancel(InputAction.CallbackContext context)
        {
            if (context.performed)
            {
                cancel.OnNext(Unit.Default);
            }
        }

        public void OnAction(InputAction.CallbackContext context)
        {
            UpdatePressed(GuideInput.FaceLeft, context);
            if (context.performed)
            {
                action.OnNext(Unit.Default);
            }
        }

        public void OnAddNode(InputAction.CallbackContext context)
        {
            if (context.performed)
            {
                addNode.OnNext(Unit.Default);
            }
        }

        /// <summary>started で点け、canceled で消す。</summary>
        /// <remarks>
        /// 使えるのはデジタルなボタンだけ。startedは actuation > 0 で飛ぶのに対し
        /// performed は押し込み閾値(既定0.5)を超えないと飛ばないので、アナログトリガー
        /// (L2/R2にあたる Lock / GrabModifier)でこれを使うと、機能が動いていない半押し域で
        /// ガイドだけが光ってしまう。あの2つは performed / canceled で拾っている。
        /// Tapのアクション(Cancel / AddNode)も canceled が来ないので使えない(pressedのremarks参照)。
        /// </remarks>
        void UpdatePressed(GuideInput input, InputAction.CallbackContext context)
        {
            if (context.started)
            {
                SetPressed(input, true);
            }
            else if (context.canceled)
            {
                SetPressed(input, false);
            }
        }

        public void OnRemoveEdge(InputAction.CallbackContext context)
        {
            UpdatePressed(GuideInput.FaceRight, context);
            if (context.started)
            {
                removeEdgeHoldId = (removeEdgeHoldId + 1) % 255;
                OnRemoveEdgeStartAsync(removeEdgeHoldId).Forget();
            }
            else if (context.canceled)
            {
                if (removeEdgeHolding)
                {
                    removeEdgeHolding = false;
                    removeEdge.OnNext(HoldState.Cancel);
                }
            }
            else if (context.performed)
            {
                removeEdgeHolding = false;
                removeEdge.OnNext(HoldState.Perform);
            }
        }

        async UniTaskVoid OnRemoveEdgeStartAsync(int id)
        {
            removeEdgeHoldId = id;
            removeEdgeHolding = true;
            await UniTask.Delay(TimeSpan.FromMilliseconds(200));
            if (removeEdgeHolding && removeEdgeHoldId == id)
            {
                removeEdge.OnNext(HoldState.Start);
            }
        }

        public void OnRemoveNode(InputAction.CallbackContext context)
        {
            UpdatePressed(GuideInput.FaceTop, context);
            if (context.started)
            {
                removeNodeHoldId = (removeNodeHoldId + 1) % 255;
                OnRemoveNodeStartAsync(removeNodeHoldId).Forget();
            }
            else if (context.canceled)
            {
                if (removeNodeHolding)
                {
                    removeNodeHolding = false;
                    removeNode.OnNext(HoldState.Cancel);
                }
            }
            else if (context.performed)
            {
                removeNodeHolding = false;
                removeNode.OnNext(HoldState.Perform);
            }
        }

        async UniTaskVoid OnRemoveNodeStartAsync(int id)
        {
            removeNodeHolding = true;
            removeNodeHoldId = id;
            await UniTask.Delay(TimeSpan.FromMilliseconds(200));
            if (removeNodeHolding && removeNodeHoldId == id)
            {
                removeNode.OnNext(HoldState.Start);
            }
        }

        /// <remarks>
        /// GrabModifierは単押しでは何もしない修飾キー。押している間の十字キーを
        /// <see cref="HandleChordNavigate"/> が拾う。
        /// initialStateCheck付きなので、押したままページを出入りしても再有効化時に
        /// performedが来て修飾キー状態が復元される。
        /// </remarks>
        public void OnGrabModifier(InputAction.CallbackContext context)
        {
            if (context.performed)
            {
                // L2/R2はアナログトリガーなので、startedを使うと半押しで光ってしまう
                // (詳細は UpdatePressed のコメント)
                SetPressed(GuideInput.UpperRight, true);
                grabModifierHeld.Value = true;
                BeginChord();
            }
            else if (context.canceled)
            {
                SetPressed(GuideInput.UpperRight, false);
                // 離した時点で倒れたままの十字キーはナビゲートに引き継がない。
                // Navigateは値が変わるまでイベントが来ないので、倒し直すまで移動しない。
                grabModifierHeld.Value = false;
            }
        }

        void BeginChord()
        {
            // 押した時点で既に倒れている十字キーは発火させない。倒し直しを待つ。
            // 斜め(判定不能)のときは方向でシードできないので、中立を観測するまで保留する。
            var seeded = ToNavigateDirection(rectorInput.Graph.Navigate.ReadValue<Vector2>());
            chordNeutralRequired = seeded is null;
            chordNavigateDirection = seeded ?? NavigateDirection.None;
            // リピート中のナビゲートも止める
            navigateInputThrottle.SetInput(Vector2.zero);
        }

        /// <remarks>L1/Vの単押しトグル。</remarks>
        public void OnMute(InputAction.CallbackContext context)
        {
            UpdatePressed(GuideInput.LowerLeft, context);
            if (context.performed)
            {
                mute.OnNext(Unit.Default);
            }
        }

        public void OnOpenNodeParameter(InputAction.CallbackContext context)
        {
            UpdatePressed(GuideInput.LowerRight, context);
            if (context.performed)
            {
                openNodeParameter.OnNext(Unit.Default);
            }
            else if (context.canceled)
            {
                closeNodeParameter.OnNext(Unit.Default);
            }
        }

        /// <remarks>
        /// 押している間は選択ノードに画面が追従する(GraphPage が LockHeld を見て始める)。
        /// Grab(R2/Option)と同時押しのときは十字キーのグループ移動が子孫ごとになり
        /// (<see cref="HandleChordNavigate"/>)、追従の方は GraphPage 側で止まる。
        /// initialStateCheck付きなので、押したままページを出入りしても performed が来て状態が復元される。
        /// </remarks>
        public void OnLock(InputAction.CallbackContext context)
        {
            if (context.performed)
            {
                SetPressed(GuideInput.UpperLeft, true);
                lockHeld.Value = true;
            }
            else if (context.canceled)
            {
                SetPressed(GuideInput.UpperLeft, false);
                lockHeld.Value = false;
            }
        }

        public void OnOpenSystem(InputAction.CallbackContext context)
        {
            if (context.performed)
            {
                openSystem.OnNext(Unit.Default);
            }
        }

        public void OnOpenScene(InputAction.CallbackContext context)
        {
            if (context.performed)
            {
                openScene.OnNext(Unit.Default);
            }
        }

        public void OnZoom(InputAction.CallbackContext context)
        {
            if (context.performed)
            {
                Zoom = context.ReadValue<float>();
            }
            else if (context.canceled)
            {
                Zoom = 0f;
            }

            // Value型なので倒している間ずっと値が来る。押下は値の有無で決まる
            SetPressed(GuideInput.Zoom, !Mathf.Approximately(Zoom, 0f));
        }

        public void OnTranslate(InputAction.CallbackContext context)
        {
            if (context.performed)
            {
                Translate = context.ReadValue<Vector2>();
            }
            else if (context.canceled)
            {
                Translate = Vector2.zero;
            }

            SetPressed(GuideInput.Pan, Translate.sqrMagnitude != 0f);
        }

        public void OnResetTransform(InputAction.CallbackContext context)
        {
            UpdatePressed(GuideInput.Reset, context);
            if (context.performed)
            {
                resetTransform.OnNext(Unit.Default);
            }
        }

        public void Initialize()
        {
            rectorInput.Graph.SetCallbacks(this);
            navigateInputThrottle.Initialize();
        }

        public void Dispose()
        {
            navigateInputThrottle.Dispose();
        }
    }
}
