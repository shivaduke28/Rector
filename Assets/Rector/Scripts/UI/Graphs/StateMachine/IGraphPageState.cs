using UnityEngine;

namespace Rector.UI.Graphs.StateMachine
{
    public interface IGraphPageState
    {
        void Navigate(Vector2 value);
        void Cancel();
        void Submit();
        void Action1();
        void Action2();
        void SubmitHoldStart();
        void SubmitHoldCancel();
        void SubmitHold();
        void Action2HoldStart();
        void Action2HoldCancel();
        void Action2Hold();
        void ToggleMute();
    }
}
