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
        readonly GroupGuideView groupGuideView;
        readonly GraphViewSettings viewSettings;
        readonly CompositeDisposable disposable = new();

        public const string AnimationClassName = "rector-graph-content-animation";

        float currentScale = 1f;
        const float MaxScale = 4f;
        const float MinScale = 0.5f;
        Vector2 offset;

        /// <summary>Resetでコンテンツを置くときの、maskの端からの余白。</summary>
        const float ResetMargin = 24f;

        /// <summary>GROUPラベルの高さ。USS (font-size 10px + padding 2px) からの実測値。</summary>
        const float GroupLabelHeight = 18f;

        /// <summary>
        /// Resetでコンテンツ原点をmaskのどこに置くか。
        /// 原点はグループ枠の左上ではなく最上段のノードの左上なので、枠の余白とその上の
        /// ラベルの分だけ下げないと、mask (overflow: hidden) に切られる。
        /// </summary>
        static readonly Vector2 ResetTranslation =
            new(ResetMargin, ResetMargin + NodeGroups.Padding + GroupLabelHeight);

        // resolvedStyle はレイアウト解決後にしか更新されないため、
        // 加減算で読み戻さずに済むよう平行移動量を保持する。
        Vector2 translation;

        Vector2 MaskSizeHalf => new(mask.resolvedStyle.width * 0.5f, mask.resolvedStyle.height * 0.5f);

        public GraphContentTransformer(VisualElement mask, VisualElement content, GraphInputAction graphInputAction,
            GroupGuideView groupGuideView, GraphViewSettings viewSettings)
        {
            this.mask = mask;
            this.content = content;
            this.graphInputAction = graphInputAction;
            this.groupGuideView = groupGuideView;
            this.viewSettings = viewSettings;
        }

        public void Initialize()
        {
            Observable.EveryUpdate(UnityFrameProvider.PostLateUpdate).Subscribe(_ =>
            {
                ApplyTranslateAndZoom();
                // グループガイドはcontentと同じtranslation/scaleから位置を決めるので、
                // 書き込み口をここ一本に絞る。Layoutは値が変わらなければ何もしない。
                groupGuideView.Layout(translation, currentScale);
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
            groupGuideView.SetAnimationEnabled(false);
        }

        void EnableAnimation()
        {
            content.AddToClassList(AnimationClassName);
            groupGuideView.SetAnimationEnabled(true);
        }

        void SetTranslation(Vector2 value)
        {
            translation = value;
            content.style.translate = value;
            // ここで一緒に書かないと、Reset のように毎フレームのループの外から呼ばれた経路で
            // content だけ先に動き、グループ枠は次のフレーム（アニメーションを戻したあと）に
            // 遅れて追いかけることになる。
            groupGuideView.Layout(translation, currentScale);
        }

        void Reset()
        {
            DisableAnimation();
            currentScale = 1f;
            offset = Vector2.zero;
            SetTranslation(ResetTranslation);
            content.style.scale = Vector3.one;
        }

        void ApplyZoom(float zoom)
        {
            var beforeScale = currentScale;
            // スティックの倒し量(±1)がそのまま速度になる。キーボード(U/O)は±1で従来速度。
            var delta = Time.deltaTime * zoom;
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
            // 常時追従の設定に加えて、Lock(R2/Tab)を握っている間だけの一時追従がある
            if (!viewSettings.FollowSelectedNode.CurrentValue && !graphInputAction.IsLockHeld) return;

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
