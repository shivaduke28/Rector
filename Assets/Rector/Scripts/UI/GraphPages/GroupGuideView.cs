using System.Collections.Generic;
using Rector.UI.LayeredGraphDrawing;
using UnityEngine;
using UnityEngine.UIElements;

namespace Rector.UI.GraphPages
{
    /// <summary>
    /// グループの区切り線とヘッダーを描く。
    /// </summary>
    /// <remarks>
    /// graph-content の兄弟として graph-mask 直下に置き、content と全く同じ平行移動と
    /// scale を掛ける。グループの矩形も content と同じ座標系のまま渡すので、
    /// パンでもズームでもノードとズレようがない。
    /// 書き込みは GraphContentTransformer からの Layout 一本に絞っている。
    /// </remarks>
    public sealed class GroupGuideView
    {
        public const string RootName = "group-guide-root";

        const string GroupClassName = "rector-group";
        const string GroupActiveClassName = "rector-group--active";
        const string BoxClassName = "rector-group-box";
        const string LabelClassName = "rector-group-label";

        readonly VisualElement root;
        readonly NodeGroups groups;
        readonly List<VisualElement> groupElements = new(NodeGroups.MaxCount);

        int activeGroup = -1;
        Vector2 lastTranslation = new(float.NaN, float.NaN);
        float lastScale = float.NaN;
        int lastRevision = -1;

        public GroupGuideView(VisualElement root, NodeGroups groups)
        {
            this.root = root;
            this.groups = groups;
            root.pickingMode = PickingMode.Ignore;
        }

        public void Layout(Vector2 translation, float scale)
        {
            var bounds = groups.Bounds;

            // 毎フレーム呼ばれるので、変化がなければ何も触らない
            if (groups.Revision == lastRevision
                && Mathf.Approximately(translation.x, lastTranslation.x)
                && Mathf.Approximately(translation.y, lastTranslation.y)
                && Mathf.Approximately(scale, lastScale))
            {
                return;
            }

            lastRevision = groups.Revision;
            lastTranslation = translation;
            lastScale = scale;

            root.style.translate = translation;
            root.style.scale = new Vector3(scale, scale, 1f);

            EnsureGroupElements(bounds.Count);

            for (var i = 0; i < bounds.Count; i++)
            {
                var element = groupElements[i];
                element.style.left = bounds[i].OriginX;
                element.style.width = bounds[i].Width;
                element.style.top = bounds[i].OriginY;
                // 0番目のラベルは絶対配置で枠の外（上）に出るので、高さは枠だけで決まる
                element[1].style.height = bounds[i].Height;
            }
        }

        public void SetActiveGroup(int group)
        {
            if (group == activeGroup) return;

            activeGroup = group;
            ApplyActiveGroup();
        }

        public void SetAnimationEnabled(bool enabled)
        {
            root.EnableInClassList(GraphContentTransformer.AnimationClassName, enabled);
        }

        void ApplyActiveGroup()
        {
            for (var i = 0; i < groupElements.Count; i++)
            {
                groupElements[i].EnableInClassList(GroupActiveClassName, i == activeGroup);
            }
        }

        void EnsureGroupElements(int count)
        {
            if (groupElements.Count == count) return;

            while (groupElements.Count < count)
            {
                var element = new VisualElement { pickingMode = PickingMode.Ignore };
                element.AddToClassList(GroupClassName);

                // ラベルは枠の上へ絶対配置で載せる (USS の bottom: 100%)
                var label = new Label($"GROUP {groupElements.Count + 1}") { pickingMode = PickingMode.Ignore };
                label.AddToClassList(LabelClassName);
                element.Add(label);

                var box = new VisualElement { pickingMode = PickingMode.Ignore };
                box.AddToClassList(BoxClassName);
                element.Add(box);

                root.Add(element);
                groupElements.Add(element);
            }

            while (groupElements.Count > count)
            {
                var last = groupElements.Count - 1;
                root.Remove(groupElements[last]);
                groupElements.RemoveAt(last);
            }

            ApplyActiveGroup();
        }
    }
}
