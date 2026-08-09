using UnityEngine;

namespace Rector.UI.GraphPages
{
    public abstract class GraphPageInputHandler
    {
        public virtual void Navigate(Vector2 value)
        {
        }

        /// <summary>
        /// 選択中のノードを隣のグループへ移す。directionは-1か1。GrabModifier(R2/Option)を押しながらの左右。
        /// </summary>
        public virtual void MoveNodeToGroup(int direction)
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
