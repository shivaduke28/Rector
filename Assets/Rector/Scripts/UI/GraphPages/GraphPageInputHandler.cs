using UnityEngine;

namespace Rector.UI.GraphPages
{
    public abstract class GraphPageInputHandler
    {
        public virtual void Navigate(Vector2 value)
        {
        }

        /// <summary>
        /// フォーカスを隣のグループへ移す。directionは-1か1。
        /// </summary>
        /// <remarks>
        /// ノード自体のグループ移動はパラメータパネルの Group 行が持つ。同じ左スティックの
        /// 操作がStateによって別の意味にならないよう、ここでは移すのはフォーカスだけにする。
        /// 実装しないStateでは何もしない。
        /// </remarks>
        public virtual void MoveGroup(int direction)
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
