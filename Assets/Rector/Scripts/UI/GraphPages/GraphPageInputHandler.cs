using UnityEngine;

namespace Rector.UI.GraphPages
{
    public abstract class GraphPageInputHandler
    {
        public virtual void Navigate(Vector2 value)
        {
        }

        /// <summary>
        /// フォーカスを隣のグループへ移す。directionは-1か1。キーボードQ/E。
        /// </summary>
        /// <remarks>
        /// ノード自体のグループ移動は <see cref="MoveNodeToGroup"/>(GrabModifier側)が持つ。
        /// 同じ操作がStateによって別の意味にならないよう、ここで移すのはフォーカスだけにする。
        /// 実装しないStateでは何もしない。
        /// </remarks>
        public virtual void MoveGroup(int direction)
        {
        }

        /// <summary>
        /// 選択中のノードを隣のグループへ移す。directionは-1か1。GrabModifier(R2/Ctrl)を押しながらの左右。
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
