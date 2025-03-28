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

        public Observable<Vector2> Navigate => navigate;
        public Observable<Unit> Submit => submit;
        public Observable<Unit> Cancel => cancel;
        Vector2 NavigateValue => rectorInput.UI.Navigate.ReadValue<Vector2>();

        IDisposable disposable;

        public UIInput(RectorInput rectorInput)
        {
            this.rectorInput = rectorInput;
        }

        void IInitializable.Initialize()
        {
            rectorInput.UI.Submit.performed += OnSubmit;
            rectorInput.UI.Cancel.performed += OnCancel;

            disposable = Observable.EveryUpdate(UnityFrameProvider.Update).Subscribe(_ => CheckNavigate());
        }


        void IDisposable.Dispose()
        {
            rectorInput.UI.Submit.performed -= OnSubmit;
            rectorInput.UI.Cancel.performed -= OnCancel;
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

        void OnSubmit(InputAction.CallbackContext ctx) => submit.OnNext(Unit.Default);
        void OnCancel(InputAction.CallbackContext ctx) => cancel.OnNext(Unit.Default);
    }
}
