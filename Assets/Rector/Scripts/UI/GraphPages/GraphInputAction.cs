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
        readonly Subject<Unit> openNodeParameter = new();
        readonly Subject<Unit> closeNodeParameter = new();
        readonly Subject<Unit> openSystem = new();
        readonly Subject<Unit> openScene = new();
        readonly Subject<Unit> resetTransform = new();

        readonly NavigateInputThrottle navigateInputThrottle = new();

        public Observable<Unit> Submit => submit;
        public Observable<Unit> Cancel => cancel;
        public Observable<Unit> Action => action;
        public Observable<Unit> AddNode => addNode;
        public Observable<HoldState> RemoveEdge => removeEdge;
        public Observable<HoldState> RemoveNode => removeNode;
        public Observable<Unit> Mute => mute;
        public Observable<Unit> OpenNodeParameter => openNodeParameter;
        public Observable<Unit> CloseNodeParameter => closeNodeParameter;
        public Observable<Unit> OpenSystem => openSystem;
        public Observable<Unit> OpenScene => openScene;
        public Observable<Unit> ResetTransform => resetTransform;
        public Observable<Vector2> Navigate => navigateInputThrottle.Navigate;

        public Vector2 Translate { get; private set; }
        public float Zoom { get; private set; }
        public bool IsNodeParameterOpen => rectorInput.Graph.OpenNodeParameter.IsPressed();

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
                openNodeParameter.OnNext(Unit.Default);
            }
            else if (context.canceled)
            {
                closeNodeParameter.OnNext(Unit.Default);
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
