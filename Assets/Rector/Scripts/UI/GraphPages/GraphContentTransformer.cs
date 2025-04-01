using System;
using Cysharp.Threading.Tasks;
using R3;
using Rector.UI.LayeredGraphDrawing;
using UnityEngine;
using UnityEngine.UIElements;

namespace Rector.UI.GraphPages
{
    public sealed class GraphContentTransformer : IInitializable, IDisposable
    {
        readonly VisualElement mask;
        readonly VisualElement content;
        readonly GraphInputAction graphInputAction;
        readonly CompositeDisposable disposable = new();

        const string AnimationClassName = "rector-graph-content-animation";

        float currentScale = 1f;
        const float MaxScale = 4f;
        const float MinScale = 0.5f;
        Vector2 offset;

        Vector2 MaskSizeHalf => new(mask.resolvedStyle.width * 0.5f, mask.resolvedStyle.height * 0.5f);

        public GraphContentTransformer(VisualElement mask, VisualElement content, GraphInputAction graphInputAction)
        {
            this.mask = mask;
            this.content = content;
            this.graphInputAction = graphInputAction;
        }

        public void Initialize()
        {
            Observable.EveryUpdate(UnityFrameProvider.PostLateUpdate).Subscribe(_ => ApplyTranslateAndZoom()).AddTo(disposable);
            graphInputAction.ResetTransform.Subscribe(_ => Reset()).AddTo(disposable);
            // UIの初期化を待ちたいので1F遅らせる
            UniTask.Create(async () =>
            {
                await UniTask.DelayFrame(1);
                Reset();
            });
        }

        void DisableAnimation()
        {
            content.RemoveFromClassList(AnimationClassName);
        }

        void EnableAnimation()
        {
            content.AddToClassList(AnimationClassName);
        }

        void Reset()
        {
            DisableAnimation();
            currentScale = 1f;
            offset = Vector2.zero;
            content.transform.position = MaskSizeHalf;
            content.transform.scale = Vector3.one;
        }

        void ApplyZoom(float zoom)
        {
            var beforeScale = currentScale;
            var delta = Time.deltaTime * Mathf.Sign(zoom);
            currentScale = Mathf.Clamp(currentScale + delta, MinScale, MaxScale);
            var scale = new Vector3(currentScale, currentScale, 1f);
            content.transform.scale = scale;

            // maskの中心が移動した分だけcontentを移動させることでmaskの中心をズームする
            var maskCenter = mask.worldBound.center;
            var contentLeftUp = new Vector2(content.worldBound.xMin, content.worldBound.yMin);
            var centerPosition = maskCenter - contentLeftUp;
            var diff = centerPosition * (currentScale / beforeScale - 1f);
            content.transform.position -= new Vector3(diff.x, diff.y);
        }


        void ApplyTranslateAndZoom()
        {
            var translate = graphInputAction.Translate;
            var zoom = graphInputAction.Zoom;

            var hasTranslate = translate.sqrMagnitude != 0f;
            var hasZoom = !Mathf.Approximately(zoom, 0f);

            if (!hasTranslate && !hasZoom)
            {
                EnableAnimation();
                return;
            }

            DisableAnimation();

            if (hasTranslate)
            {
                ApplyTranslate(translate);
            }

            if (hasZoom)
            {
                ApplyZoom(zoom);
            }
        }

        void ApplyTranslate(Vector2 translate)
        {
            var delta = new Vector3(translate.x, -translate.y, 0f) * 10f;
            content.transform.position += delta;
            offset += new Vector2(delta.x, delta.y);
        }

        public void MoveContentToMakeNodeVisible(LayeredNode node)
        {
            // left-top
            var nodePosition = node.TargetPosition * currentScale;
            content.transform.position = -nodePosition + MaskSizeHalf + offset;
        }

        public void Dispose()
        {
            disposable.Dispose();
        }
    }
}
