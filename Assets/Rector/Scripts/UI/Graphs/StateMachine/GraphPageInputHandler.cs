using UnityEngine;

namespace Rector.UI.Graphs.StateMachine
{
    public abstract class GraphPageInputHandler
    {
        public virtual void Navigate(Vector2 value)
        {
        }

        public virtual void Cancel()
        {
        }

        public virtual void Submit()
        {
        }

        public virtual void Action()
        {
        }

        public virtual void AddNode()
        {
        }

        public virtual void RemoveNode(HoldState state)
        {
        }

        public virtual void RemoveEdge(HoldState state)
        {
        }

        public virtual void Mute()
        {
        }

        public virtual void OpenNodeParameter()
        {
        }

        public virtual void CloseNodeParameter()
        {
        }
    }
}
