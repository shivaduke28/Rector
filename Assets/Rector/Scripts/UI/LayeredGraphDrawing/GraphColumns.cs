using System.Collections.Generic;
using R3;
using UnityEngine;

namespace Rector.UI.LayeredGraphDrawing
{
    public readonly struct ColumnBounds
    {
        public readonly float OriginX;
        public readonly float Width;

        public ColumnBounds(float originX, float width)
        {
            OriginX = originX;
            Width = width;
        }
    }

    /// <summary>
    /// グラフをカンバンのように縦のカラムへ分割するためのモデル。
    /// レイヤー(y)は全カラム共通のまま、レイヤー内の並び替えとx圧縮だけをカラム内に閉じる。
    /// </summary>
    /// <remarks>
    /// カラム幅はセッション中単調非減少にしている。毎回コンテンツ幅から計算し直すと、
    /// あるカラムでのノード追加・削除が右側のカラムを丸ごと横に動かしてしまい、
    /// 「土地勘を保つ」という目的そのものを損なうため。
    /// </remarks>
    public sealed class GraphColumns
    {
        public const int MinCount = 1;
        public const int MaxCount = 8;
        public const int DefaultCount = 4;

        /// <summary>カラムの最小幅。NodeView.Widthはレイアウト解決前は0なので、その揺れもここで吸収される。</summary>
        public const float MinWidth = 360f;
        public const float Gap = 60f;

        readonly ReactiveProperty<int> count = new(DefaultCount);
        public ReadOnlyReactiveProperty<int> Count => count;
        public int CurrentCount => count.Value;

        readonly float[] widths = new float[MaxCount];
        readonly List<ColumnBounds> bounds = new(MaxCount);

        /// <summary>Sortが書き込み、ColumnGuideViewが読む。要素数はCurrentCountと一致する。</summary>
        public IReadOnlyList<ColumnBounds> Bounds => bounds;

        /// <summary>Boundsが更新されるたびに増える。Viewが再描画の要否を判定するのに使う。</summary>
        public int Revision { get; private set; }

        public void SetCount(int value)
        {
            var clamped = Mathf.Clamp(value, MinCount, MaxCount);
            if (clamped == count.Value) return;

            // 幅の単調性はカラム構成が変わったらリセットする
            ResetWidths();
            count.Value = clamped;
        }

        public int Clamp(int column) => Mathf.Clamp(column, 0, count.Value - 1);

        void ResetWidths()
        {
            for (var i = 0; i < widths.Length; i++)
            {
                widths[i] = 0f;
            }
        }

        public void BeginLayout()
        {
            bounds.Clear();
            Revision++;
        }

        /// <summary>
        /// カラムの原点と幅を確定して幅を返す。カラム0から順に呼ぶこと。
        /// </summary>
        public float Place(int index, float originX, float contentWidth)
        {
            var width = Mathf.Max(MinWidth, Mathf.Max(widths[index], contentWidth));
            widths[index] = width;
            bounds.Add(new ColumnBounds(originX, width));
            return width;
        }
    }
}
