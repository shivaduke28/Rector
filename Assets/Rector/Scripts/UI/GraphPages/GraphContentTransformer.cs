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
        readonly ColumnGuideView columnGuideView;
        readonly CompositeDisposable disposable = new();

        public const string AnimationClassName = "rector-graph-content-animation";

        float currentScale = 1f;
        const float MaxScale = 4f;
        const float MinScale = 0.5f;
        Vector2 offset;

        // resolvedStyle はレイアウト解決後にしか更新されないため、
        // 加減算で読み戻さずに済むよう平行移動量を保持する。
        Vector2 translation;

        Vector2 MaskSizeHalf => new(mask.resolvedStyle.width * 0.5f, mask.resolvedStyle.height * 0.5f);

        public GraphContentTransformer(VisualElement mask, VisualElement content, GraphInputAction graphInputAction,
            ColumnGuideView columnGuideView)
        {
            this.mask = mask;
            this.content = content;
            this.graphInputAction = graphInputAction;
            this.columnGuideView = columnGuideView;
        }

        public void Initialize()
        {
            Observable.EveryUpdate(UnityFrameProvider.PostLateUpdate).Subscribe(_ =>
            {
                ApplyTranslateAndZoom();
                // カラムガイドはcontentと同じtranslation/scaleから位置を決めるので、
                // 書き込み口をここ一本に絞る。Layoutは値が変わらなければ何もしない。
                columnGuideView.Layout(translation.x, currentScale);
            }).AddTo(disposable);
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
            columnGuideView.SetAnimationEnabled(false);
        }

        void EnableAnimation()
        {
            content.AddToClassList(AnimationClassName);
            columnGuideView.SetAnimationEnabled(true);
        }

        void SetTranslation(Vector2 value)
        {
            translation = value;
            content.style.translate = value;
        }

        void Reset()
        {
            DisableAnimation();
            currentScale = 1f;
            offset = Vector2.zero;
            SetTranslation(MaskSizeHalf);
            content.style.scale = Vector3.one;
        }

        void ApplyZoom(float zoom)
        {
            var beforeScale = currentScale;
            var delta = Time.deltaTime * Mathf.Sign(zoom);
            currentScale = Mathf.Clamp(currentScale + delta, MinScale, MaxScale);
            var scale = new Vector3(currentScale, currentScale, 1f);
            content.style.scale = scale;

            // maskの中心が移動した分だけcontentを移動させることでmaskの中心をズームする
            var maskCenter = mask.worldBound.center;
            var contentLeftUp = new Vector2(content.worldBound.xMin, content.worldBound.yMin);
            var centerPosition = maskCenter - contentLeftUp;
            var diff = centerPosition * (currentScale / beforeScale - 1f);
            SetTranslation(translation - diff);
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
            var delta = new Vector2(translate.x, -translate.y) * 10f;
            SetTranslation(translation + delta);
            offset += delta;
        }

        public void MoveContentToMakeNodeVisible(LayeredNode node)
        {
            // left-top
            var nodePosition = node.TargetPosition * currentScale;
            SetTranslation(-nodePosition + MaskSizeHalf + offset);
        }

        public void Dispose()
        {
            disposable.Dispose();
        }
    }
}
