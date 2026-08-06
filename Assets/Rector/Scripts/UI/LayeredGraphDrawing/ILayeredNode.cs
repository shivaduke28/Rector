using System.Collections.Generic;
using Rector.UI.Graphs;
using UnityEngine;

namespace Rector.UI.LayeredGraphDrawing
{
    public interface ILayeredNode
    {
        // static
        NodeId Id { get; }

        // static
        int InputSlotCount { get; }

        // static
        int OutputSlotCount { get; }

        // static
        bool IsDummy { get; }

        // nealy static
        float Width { get; }

        // nealy static
        float Height { get; }

        // dynamic
        Vector2 Position { get; set; }

        // dynamic
        int Layer { get; set; }

        /// <summary>
        /// レイヤー内の位置。Sort中はカラム内ローカルの添字になる。
        /// </summary>
        // dynamic
        int Index { get; set; }

        /// <summary>
        /// 所属するカラム。並び替えとx圧縮はこの単位で閉じる。
        /// Dummy Nodeは同一カラム内のエッジにしか作られないので、エッジのカラムを引き継ぐ。
        /// </summary>
        // dynamic
        int Column { get; set; }

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
