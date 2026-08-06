using UnityEngine;

namespace Rector.UI.GraphPages
{
    public abstract class GraphPageInputHandler
    {
        public virtual void Navigate(Vector2 value)
        {
        }

        /// <summary>
        /// 選択中のノードをカラム間で移動する。directionは-1か1。
        /// ノード選択中以外は何もしないので、他のStateでは自然にブロックされる。
        /// </summary>
        public virtual void MoveColumn(int direction)
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
