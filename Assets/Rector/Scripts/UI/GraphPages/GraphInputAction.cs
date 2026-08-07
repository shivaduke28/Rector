using System;
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
        readonly Subject<Unit> muteChord = new();
        readonly Subject<Vector2Int> navigateInGroup = new();
        readonly Subject<Unit> lockStarted = new();
        readonly Subject<Unit> openNodeParameter = new();
        readonly Subject<Unit> closeNodeParameter = new();
        readonly Subject<Unit> openSystem = new();
        readonly Subject<Unit> openScene = new();
        readonly Subject<Unit> resetTransform = new();
        readonly Subject<int> moveGroup = new();
        readonly Subject<int> moveNodeToGroup = new();

        readonly NavigateInputThrottle navigateInputThrottle = new();

        public Observable<Unit> Submit => submit;
        public Observable<Unit> Cancel => cancel;
        public Observable<Unit> Action => action;
        public Observable<Unit> AddNode => addNode;
        public Observable<HoldState> RemoveEdge => removeEdge;
        public Observable<HoldState> RemoveNode => removeNode;
        public Observable<Unit> Mute => mute;
        public Observable<Unit> MuteChord => muteChord;
        public Observable<Vector2Int> NavigateInGroup => navigateInGroup;
        public Observable<Unit> LockStarted => lockStarted;
        public Observable<Unit> OpenNodeParameter => openNodeParameter;
        public Observable<Unit> CloseNodeParameter => closeNodeParameter;
        public Observable<Unit> OpenSystem => openSystem;
        public Observable<Unit> OpenScene => openScene;
        public Observable<Unit> ResetTransform => resetTransform;
        public Observable<Vector2> Navigate => navigateInputThrottle.Navigate;
        public Observable<int> MoveGroup => moveGroup;
        public Observable<int> MoveNodeToGroup => moveNodeToGroup;

        public Vector2 Translate { get; private set; }
        public float Zoom { get; private set; }
        public bool IsNodeParameterOpen => rectorInput.Graph.OpenNodeParameter.IsPressed();
        public bool IsLockHeld => rectorInput.Graph.Lock.IsPressed();

        const float DirectionThreshold = 0.5f;
        int moveGroupDirection;

        enum NavigateDirection
        {
            None,
            Up,
            Down,
            Left,
            Right,
        }

        bool navModifierHeld;
        bool grabModifierHeld;
        NavigateDirection chordNavigateDirection;
        bool nodeParameterOpenSuppressed;

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
            nodeParameterOpenSuppressed = false;
            // moveGroupDirection はここでリセットしない。MoveGroup は initialStateCheck 付きなので、
            // 倒したまま抜けて戻ると再有効化時に performed が来る。0に戻しておくと
            // 「中立から倒れた」と誤判定して、触っていないのにグループが1つ飛ぶ。
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
            if (navModifierHeld || grabModifierHeld)
            {
                HandleChordNavigate(value);
                return;
            }

            navigateInputThrottle.SetInput(value);
        }

        /// <remarks>
        /// 修飾キーを押している間は十字キーを別コマンドとして扱う。
        /// NavModifier(L1/Option): 上下左右ともグループ内に閉じたフォーカス移動。
        /// GrabModifier(R2/Ctrl): 左右は選択ノードのグループ移動。両方押しはGrab優先。
        /// どれも離散的な操作なのでリピートさせず、方向が新しく確定した瞬間だけ発火させる。
        /// </remarks>
        void HandleChordNavigate(Vector2 value)
        {
            // 斜めは判定保留(前の方向を維持)。Noneに落とすと、右→右下→右と転がっただけで
            // 「倒し直した」ことになりグループ内移動が二重発火する。逆に縦横どちらかに寄せると、
            // 右を押した指が角に転がった瞬間に別コマンドが誤発火する。
            var direction = ToNavigateDirection(value);
            if (direction is not { } d || d == chordNavigateDirection) return;

            chordNavigateDirection = d;
            if (grabModifierHeld)
            {
                switch (d)
                {
                    case NavigateDirection.Left:
                        moveNodeToGroup.OnNext(-1);
                        break;
                    case NavigateDirection.Right:
                        moveNodeToGroup.OnNext(1);
                        break;
                }
            }
            else
            {
                switch (d)
                {
                    case NavigateDirection.Left:
                        navigateInGroup.OnNext(new Vector2Int(-1, 0));
                        break;
                    case NavigateDirection.Right:
                        navigateInGroup.OnNext(new Vector2Int(1, 0));
                        break;
                    case NavigateDirection.Up:
                        navigateInGroup.OnNext(new Vector2Int(0, 1));
                        break;
                    case NavigateDirection.Down:
                        navigateInGroup.OnNext(new Vector2Int(0, -1));
                        break;
                }
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

        /// <remarks>
        /// グループ移動は離散的な操作なので、Navigateのようなリピートはさせない。
        /// スティックが中立から左右に倒れた瞬間だけ発火させる。
        /// </remarks>
        public void OnMoveGroup(InputAction.CallbackContext context)
        {
            var direction = context.canceled ? 0 : ToHorizontalDirection(context.ReadValue<Vector2>());
            if (direction == moveGroupDirection) return;

            moveGroupDirection = direction;
            if (direction != 0)
            {
                moveGroup.OnNext(direction);
            }
        }

        static int ToHorizontalDirection(Vector2 value)
        {
            // 縦入力と、倒しきっていない入力は無視する
            if (Mathf.Abs(value.x) < DirectionThreshold) return 0;
            if (Mathf.Abs(value.x) <= Mathf.Abs(value.y)) return 0;
            return value.x > 0 ? 1 : -1;
        }

        public void OnSubmit(InputAction.CallbackContext context)
        {
            if (context.performed)
            {
                submit.OnNext(Unit.Default);
            }
        }

        public void OnCancel(InputAction.CallbackContext context)
        {
            if (context.performed)
            {
                cancel.OnNext(Unit.Default);
            }
        }

        public void OnAction(InputAction.CallbackContext context)
        {
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

        public void OnRemoveEdge(InputAction.CallbackContext context)
        {
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
        /// NavModifierは単押しでは何もしない修飾キー。押している間の十字キーを
        /// <see cref="HandleChordNavigate"/> が拾う。OpenNodeParameter(R1/Shift)と
        /// 重ねるとミュートトグルになる(押し順非依存)。
        /// initialStateCheck付きなので、押したままページを出入りしても再有効化時に
        /// performedが来て修飾キー状態が復元される。
        /// </remarks>
        public void OnNavModifier(InputAction.CallbackContext context)
        {
            if (context.performed)
            {
                navModifierHeld = true;
                BeginChord();
                // パネルを開いている指に後から重ねた場合もミュートにする
                if (rectorInput.Graph.OpenNodeParameter.IsPressed())
                {
                    muteChord.OnNext(Unit.Default);
                }
            }
            else if (context.canceled)
            {
                // 離した時点で倒れたままの十字キーはナビゲートに引き継がない。
                // Navigateは値が変わるまでイベントが来ないので、倒し直すまで移動しない。
                navModifierHeld = false;
            }
        }

        public void OnGrabModifier(InputAction.CallbackContext context)
        {
            if (context.performed)
            {
                grabModifierHeld = true;
                BeginChord();
            }
            else if (context.canceled)
            {
                grabModifierHeld = false;
            }
        }

        void BeginChord()
        {
            // 押した時点で既に倒れている十字キーは発火させない。倒し直しを待つ。
            chordNavigateDirection = ToNavigateDirection(rectorInput.Graph.Navigate.ReadValue<Vector2>()) ?? NavigateDirection.None;
            // リピート中のナビゲートも止める
            navigateInputThrottle.SetInput(Vector2.zero);
        }

        /// <remarks>キーボード(V)専用の単押しトグル。ゲームパッドはL1+R1(MuteChord)で同じ操作。</remarks>
        public void OnMute(InputAction.CallbackContext context)
        {
            if (context.performed)
            {
                mute.OnNext(Unit.Default);
            }
        }

        public void OnOpenNodeParameter(InputAction.CallbackContext context)
        {
            if (context.performed)
            {
                // NavModifier(L1/Option)を押しながらのときはミュートトグルで、パネルは開かない
                if (navModifierHeld)
                {
                    nodeParameterOpenSuppressed = true;
                    muteChord.OnNext(Unit.Default);
                    return;
                }

                openNodeParameter.OnNext(Unit.Default);
            }
            else if (context.canceled)
            {
                // ミュートとして消費した押下は、開いていないパネルを閉じない
                if (nodeParameterOpenSuppressed)
                {
                    nodeParameterOpenSuppressed = false;
                    return;
                }

                closeNodeParameter.OnNext(Unit.Default);
            }
        }

        public void OnLock(InputAction.CallbackContext context)
        {
            if (context.performed)
            {
                lockStarted.OnNext(Unit.Default);
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
        }

        public void OnResetTransform(InputAction.CallbackContext context)
        {
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
