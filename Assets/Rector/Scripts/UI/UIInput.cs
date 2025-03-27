using System;
using R3;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Rector.UI
{
    public sealed class UIInput : IInitializable, IDisposable
    {
        readonly RectorInput rectorInput;
        readonly Subject<Vector2> navigate = new();
        readonly Subject<Unit> submit = new();
        readonly Subject<Unit> cancel = new();
        readonly Subject<Unit> action1 = new();
        readonly Subject<Unit> action2 = new();
        readonly Subject<Unit> submitHoldStart = new();
        readonly Subject<Unit> submitHold = new();
        readonly Subject<Unit> submitHoldCancel = new();
        readonly Subject<Unit> action2HoldStart = new();
        readonly Subject<Unit> action2Hold = new();
        readonly Subject<Unit> action2HoldCancel = new();
        readonly Subject<bool> rightSideBar = new();
        readonly Subject<Unit> toggleMute = new();
        readonly Subject<Unit> system = new();
        readonly Subject<Unit> scene = new();
        readonly Subject<Unit> resetGraphPosition = new();

        public Observable<Vector2> Navigate => navigate;
        public Observable<Unit> Submit => submit;
        public Observable<Unit> Cancel => cancel;
        public Observable<Unit> Action1 => action1;
        public Observable<Unit> Action2 => action2;
        public Observable<Unit> SubmitHoldStart => submitHoldStart;
        public Observable<Unit> SubmitHold => submitHold;
        public Observable<Unit> SubmitHoldCancel => submitHoldCancel;
        public Observable<Unit> Action2HoldStart => action2HoldStart;
        public Observable<Unit> Action2Hold => action2Hold;
        public Observable<Unit> Action2HoldCancel => action2HoldCancel;
        public Observable<bool> RightSideBar => rightSideBar;
        public Observable<Unit> ToggleMute => toggleMute;
        public Observable<Unit> System => system;
        public Observable<Unit> Scene => scene;
        public Observable<Unit> ResetGraphPosition => resetGraphPosition;
        public float ZoomValue => rectorInput.UI.Zoom.ReadValue<float>();
        public Vector2 TranslateValue => rectorInput.UI.Translate.ReadValue<Vector2>();
        public Vector2 NavigateValue => rectorInput.UI.Navigate.ReadValue<Vector2>();

        IDisposable disposable;

        public UIInput(RectorInput rectorInput)
        {
            this.rectorInput = rectorInput;
        }

        void IInitializable.Initialize()
        {
            // rectorInput.UI.Navigate.performed += OnNavigate;
            rectorInput.UI.Submit.performed += OnSubmit;
            rectorInput.UI.Cancel.performed += OnCancel;
            rectorInput.UI.Action1.performed += OnAction1;
            rectorInput.UI.Action2.performed += OnAction2;
            rectorInput.UI.Submit.canceled += OnSubmitHoldStart;
            rectorInput.UI.SubmitHold.performed += OnSubmitHold;
            rectorInput.UI.SubmitHold.canceled += OnSubmitHoldCancel;
            rectorInput.UI.Action2.canceled += OnAction2HoldStart;
            rectorInput.UI.Action2Hold.performed += OnAction2Hold;
            rectorInput.UI.Action2Hold.canceled += OnAction2HoldCancel;
            rectorInput.UI.RightSideBar.performed += OnRightSideBar;
            rectorInput.UI.RightSideBar.canceled += OnRightSideBarCancel;
            rectorInput.UI.Mute.performed += OnMute;
            rectorInput.UI.System.performed += OnMenu;
            rectorInput.UI.Scene.performed += OnScene;
            rectorInput.UI.ResetGraphPosition.performed += OnResetGraphPosition;

            disposable = Observable.EveryUpdate(UnityFrameProvider.Update).Subscribe(_ => CheckNavigate());
        }


        void IDisposable.Dispose()
        {
            // rectorInput.UI.Navigate.performed -= OnNavigate;
            rectorInput.UI.Submit.performed -= OnSubmit;
            rectorInput.UI.Cancel.performed -= OnCancel;
            rectorInput.UI.Action1.performed -= OnAction1;
            rectorInput.UI.Action2.performed -= OnAction2;
            rectorInput.UI.Submit.canceled -= OnSubmitHoldStart;
            rectorInput.UI.SubmitHold.performed -= OnSubmitHold;
            rectorInput.UI.SubmitHold.canceled -= OnSubmitHoldCancel;
            rectorInput.UI.Action2.canceled -= OnAction2HoldStart;
            rectorInput.UI.Action2Hold.performed -= OnAction2Hold;
            rectorInput.UI.Action2Hold.canceled -= OnAction2HoldCancel;
            rectorInput.UI.RightSideBar.performed -= OnRightSideBar;
            rectorInput.UI.RightSideBar.canceled -= OnRightSideBarCancel;
            rectorInput.UI.Mute.performed -= OnMute;
            rectorInput.UI.System.performed -= OnMenu;
            rectorInput.UI.Scene.performed -= OnScene;
            rectorInput.UI.ResetGraphPosition.performed -= OnResetGraphPosition;

            disposable?.Dispose();
        }


        NavigateDirection lastDirection = NavigateDirection.None;
        const float InitialDelay = 0.4f;
        const float RepeatDelay = 0.05f;

        float delay = InitialDelay;

        // updateで呼ぶ
        void CheckNavigate()
        {
            var input = NavigateValue;
            var direction = ToDirection(input);

            // submitボタンは長押しがある関係で離したときに発火する
            // submitボタンを押している状態でnavigateを入力して
            // slotを誤選択することが多いのでフィルタする（長押しやめた方がいいかもしれん）
            if (rectorInput.UI.Submit.IsPressed())
            {
                direction = NavigateDirection.None;
            }

            if (direction == NavigateDirection.None)
            {
                lastDirection = NavigateDirection.None;
                return;
            }

            if (direction != lastDirection)
            {
                delay = InitialDelay;
                lastDirection = direction;
                navigate.OnNext(FromDirection(direction));
                return;
            }

            delay -= Time.deltaTime;
            if (delay > 0)
            {
                return;
            }

            delay = RepeatDelay;
            navigate.OnNext(FromDirection(direction));
        }

        static NavigateDirection ToDirection(Vector2 value)
        {
            if (value.sqrMagnitude == 0f)
            {
                return NavigateDirection.None;
            }

            if (Mathf.Abs(value.x) > Mathf.Abs(value.y))
            {
                return value.x > 0 ? NavigateDirection.Right : NavigateDirection.Left;
            }
            else
            {
                return value.y > 0 ? NavigateDirection.Up : NavigateDirection.Down;
            }
        }

        Vector2 FromDirection(NavigateDirection direction)
        {
            switch (direction)
            {
                case NavigateDirection.Up:
                    return Vector2.up;
                case NavigateDirection.Down:
                    return Vector2.down;
                case NavigateDirection.Left:
                    return Vector2.left;
                case NavigateDirection.Right:
                    return Vector2.right;
                default:
                    return Vector2.zero;
            }
        }

        enum NavigateDirection
        {
            None,
            Up,
            Down,
            Left,
            Right,
        }

        // void OnNavigate(InputAction.CallbackContext ctx)
        // {
        //     // submitボタンは長押しがある関係で離したときに発火する
        //     // submitボタンを押している状態でnavigateを入力して
        //     // slotを誤選択することが多いのでフィルタする（長押しやめた方がいいかもしれん）
        //     if (rectorInput.UI.Submit.IsPressed())
        //     {
        //         return;
        //     }
        //
        //     navigate.OnNext(ctx.ReadValue<Vector2>());
        // }

        void OnSubmit(InputAction.CallbackContext ctx) => submit.OnNext(Unit.Default);
        void OnCancel(InputAction.CallbackContext ctx) => cancel.OnNext(Unit.Default);
        void OnAction1(InputAction.CallbackContext ctx) => action1.OnNext(Unit.Default);
        void OnAction2(InputAction.CallbackContext ctx) => action2.OnNext(Unit.Default);
        void OnSubmitHoldStart(InputAction.CallbackContext ctx) => submitHoldStart.OnNext(Unit.Default);
        void OnSubmitHold(InputAction.CallbackContext ctx) => submitHold.OnNext(Unit.Default);
        void OnSubmitHoldCancel(InputAction.CallbackContext ctx) => submitHoldCancel.OnNext(Unit.Default);
        void OnAction2HoldStart(InputAction.CallbackContext ctx) => action2HoldStart.OnNext(Unit.Default);
        void OnAction2Hold(InputAction.CallbackContext ctx) => action2Hold.OnNext(Unit.Default);
        void OnAction2HoldCancel(InputAction.CallbackContext ctx) => action2HoldCancel.OnNext(Unit.Default);
        void OnRightSideBar(InputAction.CallbackContext ctx) => rightSideBar.OnNext(true);
        void OnRightSideBarCancel(InputAction.CallbackContext ctx) => rightSideBar.OnNext(false);
        void OnMute(InputAction.CallbackContext ctx) => toggleMute.OnNext(Unit.Default);
        void OnMenu(InputAction.CallbackContext ctx) => system.OnNext(Unit.Default);
        void OnScene(InputAction.CallbackContext ctx) => scene.OnNext(Unit.Default);
        void OnResetGraphPosition(InputAction.CallbackContext ctx) => resetGraphPosition.OnNext(Unit.Default);
    }
}
