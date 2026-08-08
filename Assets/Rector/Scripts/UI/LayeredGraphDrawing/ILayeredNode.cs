using System.Collections.Generic;
using Rector.UI.Graphs;
using UnityEngine;

namespace Rector.UI.LayeredGraphDrawing
{
    /// <remarks>
    /// Id/InputSlotCount/OutputSlotCount/IsDummy はノードの生涯変わらない。
    /// Width/Height はレイアウトが解決されるまで確定しない(解決前は NaN)。
    /// Position/Layer/Index/Group/Parents/Children は Sort が書き込む。
    /// </remarks>
    public interface ILayeredNode
    {
        NodeId Id { get; }

        int InputSlotCount { get; }

        int OutputSlotCount { get; }

        bool IsDummy { get; }

        float Width { get; }

        float Height { get; }

        Vector2 Position { get; set; }

        int Layer { get; set; }

        /// <summary>
        /// レイヤー内の位置。Sort中はグループ内ローカルの添字になる。
        /// </summary>
        int Index { get; set; }

        /// <summary>
        /// 所属するグループ。並び替えとx圧縮はこの単位で閉じる。
        /// Dummy Nodeは同一グループ内のエッジにしか作られないので、エッジのグループを引き継ぐ。
        /// </summary>
        int Group { get; set; }

        /// <summary>
        /// Dummy Nodeを加味した親の配列 (Short Edge)
        /// ソート中に値を入れる
        /// </summary>
        List<(ILayeredNode Node, int SlotIndex)> Parents { get; }

        /// <summary>
        /// Dummy Nodeを加味した子の配列 (Short Edge)
        /// ソート中に値を入れる
        /// </summary>
        List<(ILayeredNode Node, int SlotIndex)> Children { get; }
    }
}
