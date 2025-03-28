using System;
using R3;
using UnityEngine;

namespace Rector.UI
{
    public sealed class NavigateInputThrottle : IInitializable, IDisposable
    {
        enum NavigateDirection
        {
            None,
            Up,
            Down,
            Left,
            Right,
        }

        readonly Subject<Vector2> navigate = new();
        public Observable<Vector2> Navigate => navigate;


        Vector2 inputValue;
        NavigateDirection lastDirection = NavigateDirection.None;
        IDisposable disposable;

        const float InitialDelay = 0.4f;
        const float RepeatDelay = 0.05f;

        float delay = InitialDelay;

        public void SetInput(Vector2 value)
        {
            inputValue = value;
        }


        void CheckNavigate()
        {
            var input = inputValue;
            var direction = ToDirection(input);

            if (direction == NavigateDirection.None)
            {
                lastDirection = NavigateDirection.None;
                return;
            }

            if (direction != lastDirection)
            {
                delay = InitialDelay;
                lastDirection = direction;
                navigate.OnNext(ToVector2(direction));
                return;
            }

            delay -= Time.deltaTime;
            if (delay > 0)
            {
                return;
            }

            delay = RepeatDelay;
            navigate.OnNext(ToVector2(direction));
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

        static Vector2 ToVector2(NavigateDirection direction)
        {
            return direction switch
            {
                NavigateDirection.Up => Vector2.up,
                NavigateDirection.Down => Vector2.down,
                NavigateDirection.Left => Vector2.left,
                NavigateDirection.Right => Vector2.right,
                _ => Vector2.zero
            };
        }

        public void Initialize()
        {
            disposable = Observable.EveryUpdate(UnityFrameProvider.Update).Subscribe(_ => CheckNavigate());
        }


        public void Dispose()
        {
            disposable?.Dispose();
        }
    }
}
