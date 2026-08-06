using System.Collections.Generic;
using R3;
using UnityEngine;

namespace Rector.UI.LayeredGraphDrawing
{
    /// <summary>
    /// グループの枠。content と同じ座標系で、ノードを囲む矩形を表す。
    /// </summary>
    public readonly struct GroupBounds
    {
        public readonly float OriginX;
        public readonly float Width;
        public readonly float OriginY;
        public readonly float Height;

        public GroupBounds(float originX, float width, float originY, float height)
        {
            OriginX = originX;
            Width = width;
            OriginY = originY;
            Height = height;
        }
    }

    /// <summary>
    /// グラフをカンバンのように縦のグループへ分割するためのモデル。
    /// レイヤー(y)は全グループ共通のまま、レイヤー内の並び替えとx圧縮だけをグループ内に閉じる。
    /// </summary>
    /// <remarks>
    /// グループ幅は毎回コンテンツ幅から計算する。一度広がった幅を保つ（単調非減少にする）と
    /// 右のグループが動かなくなる代わりに、ノードを消したあとの空白が残り続けるので採らない。
    /// 幅が変わると右のグループはずれるが、グループ内の並びは動かないので土地勘は保たれる。
    /// </remarks>
    public sealed class NodeGroups
    {
        public const int MinCount = 1;
        public const int MaxCount = 8;
        public const int DefaultCount = 4;

        /// <summary>グループの最小幅。NodeView.Widthはレイアウト解決前は0なので、その揺れもここで吸収される。</summary>
        public const float MinWidth = 240f;

        /// <summary>グループの左右の内側の余白。左borderとノードがくっついて見えなくなるのを防ぐ。</summary>
        public const float Padding = 24f;

        const string PrefsKey = "Rector_NodeGroupCount";

        readonly ReactiveProperty<int> count;
        public ReadOnlyReactiveProperty<int> Count => count;
        public int CurrentCount => count.Value;

        public NodeGroups()
        {
            // 保存された値が範囲外でも壊れないようにclampして読む
            count = new ReactiveProperty<int>(Mathf.Clamp(PlayerPrefs.GetInt(PrefsKey, DefaultCount), MinCount, MaxCount));
            ResetBounds();
        }

        readonly List<GroupBounds> bounds = new(MaxCount);

        /// <summary>
        /// Sortが書き込み、GroupGuideViewが読む。要素数は常にCurrentCountと一致する。
        /// </summary>
        /// <remarks>
        /// Sortは表示中しか走らないので、Sortを待って埋めるとグループ数を変えてから最初のSortまでの間
        /// 要素数が食い違う。設定画面はグラフを閉じてから開くため、その隙間は実際に踏める。
        /// </remarks>
        public IReadOnlyList<GroupBounds> Bounds => bounds;

        /// <summary>Boundsが更新されるたびに増える。Viewが再描画の要否を判定するのに使う。</summary>
        public int Revision { get; private set; }

        public void SetCount(int value)
        {
            var clamped = Mathf.Clamp(value, MinCount, MaxCount);
            if (clamped == count.Value) return;

            count.Value = clamped;
            PlayerPrefs.SetInt(PrefsKey, clamped);
            ResetBounds();
        }

        /// <summary>
        /// 次のSortまでのつなぎとして、最小幅で並べた枠を作る。
        /// </summary>
        void ResetBounds()
        {
            bounds.Clear();
            for (var i = 0; i < count.Value; i++)
            {
                bounds.Add(new GroupBounds(i * MinWidth, MinWidth, -Padding, Padding * 2f));
            }

            Revision++;
        }

        /// <summary>
        /// グループ数より大きい番号を、実際に描かれる末尾のグループへ畳む。
        /// </summary>
        /// <remarks>
        /// 畳むだけでノードのGroupは書き換えない。書き換えるとグループ数を戻しても
        /// 並びが復元できず、undoのないアプリで取り返しがつかなくなる。
        /// </remarks>
        public int Fold(int group) => Mathf.Clamp(group, 0, count.Value - 1);

        /// <summary>グループ番号をループさせる。右端で右に進むと左端に戻る。</summary>
        public int Wrap(int group)
        {
            var n = count.Value;
            return ((group % n) + n) % n;
        }

        public void BeginLayout()
        {
            bounds.Clear();
            Revision++;
        }

        /// <summary>
        /// グループの枠を確定して幅を返す。グループ0から順に呼ぶこと。
        /// </summary>
        /// <param name="contentTop">ノードの上端。ノードがないグループでは0。</param>
        /// <param name="contentHeight">ノードの上端から下端まで。ノードがないグループでは0。</param>
        public float Place(float originX, float contentWidth, float contentTop, float contentHeight)
        {
            var width = Mathf.Max(MinWidth, contentWidth + Padding * 2f);
            bounds.Add(new GroupBounds(originX, width, contentTop - Padding, contentHeight + Padding * 2f));
            return width;
        }
    }
}
