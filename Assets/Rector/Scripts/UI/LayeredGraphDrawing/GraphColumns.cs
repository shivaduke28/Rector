using System.Collections.Generic;
using R3;
using UnityEngine;

namespace Rector.UI.LayeredGraphDrawing
{
    /// <summary>
    /// カラムの枠。content と同じ座標系で、ノードを囲む矩形を表す。
    /// </summary>
    public readonly struct ColumnBounds
    {
        public readonly float OriginX;
        public readonly float Width;
        public readonly float OriginY;
        public readonly float Height;

        public ColumnBounds(float originX, float width, float originY, float height)
        {
            OriginX = originX;
            Width = width;
            OriginY = originY;
            Height = height;
        }
    }

    /// <summary>
    /// グラフをカンバンのように縦のカラムへ分割するためのモデル。
    /// レイヤー(y)は全カラム共通のまま、レイヤー内の並び替えとx圧縮だけをカラム内に閉じる。
    /// </summary>
    /// <remarks>
    /// カラム幅は毎回コンテンツ幅から計算する。一度広がった幅を保つ（単調非減少にする）と
    /// 右のカラムが動かなくなる代わりに、ノードを消したあとの空白が残り続けるので採らない。
    /// 幅が変わると右のカラムはずれるが、カラム内の並びは動かないので土地勘は保たれる。
    /// </remarks>
    public sealed class GraphColumns
    {
        public const int MinCount = 1;
        public const int MaxCount = 8;
        public const int DefaultCount = 4;

        /// <summary>カラムの最小幅。NodeView.Widthはレイアウト解決前は0なので、その揺れもここで吸収される。</summary>
        public const float MinWidth = 240f;

        /// <summary>カラムの左右の内側の余白。左borderとノードがくっついて見えなくなるのを防ぐ。</summary>
        public const float Padding = 24f;

        readonly ReactiveProperty<int> count = new(DefaultCount);
        public ReadOnlyReactiveProperty<int> Count => count;
        public int CurrentCount => count.Value;

        readonly List<ColumnBounds> bounds = new(MaxCount);

        /// <summary>Sortが書き込み、ColumnGuideViewが読む。要素数はCurrentCountと一致する。</summary>
        public IReadOnlyList<ColumnBounds> Bounds => bounds;

        /// <summary>Boundsが更新されるたびに増える。Viewが再描画の要否を判定するのに使う。</summary>
        public int Revision { get; private set; }

        public void SetCount(int value)
        {
            var clamped = Mathf.Clamp(value, MinCount, MaxCount);
            if (clamped == count.Value) return;

            count.Value = clamped;
        }

        /// <summary>カラム番号をループさせる。右端で右に進むと左端に戻る。</summary>
        public int Wrap(int column)
        {
            var n = count.Value;
            return ((column % n) + n) % n;
        }

        public void BeginLayout()
        {
            bounds.Clear();
            Revision++;
        }

        /// <summary>
        /// カラムの枠を確定して幅を返す。カラム0から順に呼ぶこと。
        /// </summary>
        /// <param name="contentTop">ノードの上端。ノードがないカラムでは0。</param>
        /// <param name="contentHeight">ノードの上端から下端まで。ノードがないカラムでは0。</param>
        public float Place(float originX, float contentWidth, float contentTop, float contentHeight)
        {
            var width = Mathf.Max(MinWidth, contentWidth + Padding * 2f);
            bounds.Add(new ColumnBounds(originX, width, contentTop - Padding, contentHeight + Padding * 2f));
            return width;
        }
    }
}
