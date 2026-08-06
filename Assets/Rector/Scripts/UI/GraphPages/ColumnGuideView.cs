using System.Collections.Generic;
using Rector.UI.LayeredGraphDrawing;
using UnityEngine;
using UnityEngine.UIElements;

namespace Rector.UI.GraphPages
{
    /// <summary>
    /// カラムの区切り線とヘッダーを描く。
    /// </summary>
    /// <remarks>
    /// graph-content の兄弟として graph-mask 直下に置き、content と同じ scale と
    /// 同じ水平方向の平行移動を掛ける。y だけ動かさないことでヘッダーが上に貼り付く。
    /// カラムの left/width は content と同じ座標系のまま渡すので、ズームでズレようがない。
    /// 書き込みは GraphContentTransformer からの Layout 一本に絞っている。
    /// </remarks>
    public sealed class ColumnGuideView
    {
        public const string RootName = "column-guide-root";

        const string ColumnClassName = "rector-column";
        const string ColumnActiveClassName = "rector-column--active";
        const string LabelClassName = "rector-column-label";

        readonly VisualElement root;
        readonly GraphColumns columns;
        readonly List<VisualElement> columnElements = new(GraphColumns.MaxCount);

        int activeColumn = -1;
        float lastTranslationX = float.NaN;
        float lastScale = float.NaN;
        int lastRevision = -1;

        public ColumnGuideView(VisualElement root, GraphColumns columns)
        {
            this.root = root;
            this.columns = columns;
            root.pickingMode = PickingMode.Ignore;
        }

        public void Layout(float translationX, float scale)
        {
            var bounds = columns.Bounds;

            // 毎フレーム呼ばれるので、変化がなければ何も触らない
            if (columns.Revision == lastRevision
                && Mathf.Approximately(translationX, lastTranslationX)
                && Mathf.Approximately(scale, lastScale))
            {
                return;
            }

            lastRevision = columns.Revision;
            lastTranslationX = translationX;
            lastScale = scale;

            root.style.translate = new Vector2(translationX, 0f);
            root.style.scale = new Vector3(scale, scale, 1f);

            EnsureColumnElements(bounds.Count);

            for (var i = 0; i < bounds.Count; i++)
            {
                var element = columnElements[i];
                element.style.left = bounds[i].OriginX;
                element.style.width = bounds[i].Width;
            }
        }

        public void SetActiveColumn(int column)
        {
            if (column == activeColumn) return;

            activeColumn = column;
            ApplyActiveColumn();
        }

        public void SetAnimationEnabled(bool enabled)
        {
            root.EnableInClassList(GraphContentTransformer.AnimationClassName, enabled);
        }

        void ApplyActiveColumn()
        {
            for (var i = 0; i < columnElements.Count; i++)
            {
                columnElements[i].EnableInClassList(ColumnActiveClassName, i == activeColumn);
            }
        }

        void EnsureColumnElements(int count)
        {
            if (columnElements.Count == count) return;

            while (columnElements.Count < count)
            {
                var element = new VisualElement { pickingMode = PickingMode.Ignore };
                element.AddToClassList(ColumnClassName);

                var label = new Label($"COLUMN {columnElements.Count + 1}") { pickingMode = PickingMode.Ignore };
                label.AddToClassList(LabelClassName);
                element.Add(label);

                root.Add(element);
                columnElements.Add(element);
            }

            while (columnElements.Count > count)
            {
                var last = columnElements.Count - 1;
                root.Remove(columnElements[last]);
                columnElements.RemoveAt(last);
            }

            ApplyActiveColumn();
        }
    }
}
