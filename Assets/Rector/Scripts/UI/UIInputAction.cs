using System;
using R3;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.InputSystem;

namespace Rector.UI
{
    public interface IUIInputHandler
    {
        void OnNavigate(Vector2 value);
        void OnSubmit();
        void OnCancel();
    }

    public sealed class UIInputAction : RectorInput.IUIActions, IInitializable, IDisposable
    {
        readonly RectorInput rectorInput;
        readonly NavigateInputThrottle navigateInputThrottle = new();
        readonly CompositeDisposable disposables = new();

        IUIInputHandler inputHandler;

        public void Register(IUIInputHandler handler)
        {
            Assert.IsNotNull(handler);
            inputHandler = handler;
            rectorInput.UI.Enable();
        }

        public void Unregister(IUIInputHandler handler)
        {
            if (inputHandler == handler)
            {
                inputHandler = null;
                rectorInput.UI.Disable();
            }
        }

        public UIInputAction(RectorInput rectorInput)
        {
            this.rectorInput = rectorInput;
        }

        public void OnNavigate(InputAction.CallbackContext context)
        {
            if (context.performed)
            {
                navigateInputThrottle.SetInput(context.ReadValue<Vector2>());
            }
            else if (context.canceled)
            {
                navigateInputThrottle.SetInput(Vector2.zero);
            }
        }

        public void OnSubmit(InputAction.CallbackContext context)
        {
            if (context.performed)
            {
                inputHandler?.OnSubmit();
            }
        }

        public void OnCancel(InputAction.CallbackContext context)
        {
            if (context.performed)
            {
                inputHandler?.OnCancel();
            }
        }

        public void OnPoint(InputAction.CallbackContext context)
        {
        }

        public void OnClick(InputAction.CallbackContext context)
        {
        }

        public void Initialize()
        {
            rectorInput.UI.SetCallbacks(this);
            navigateInputThrottle.Initialize();
            navigateInputThrottle.Navigate.Subscribe(OnNavigate).AddTo(disposables);
        }

        void OnNavigate(Vector2 value) => inputHandler?.OnNavigate(value);

        public void Dispose()
        {
            disposables.Dispose();
            navigateInputThrottle.Dispose();
        }
    }
}
