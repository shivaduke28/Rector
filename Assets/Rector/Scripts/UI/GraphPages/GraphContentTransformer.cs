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
        readonly CompositeDisposable disposable = new();

        public const string AnimationClassName = "rector-graph-content-animation";

        float currentScale = 1f;
        const float MaxScale = 4f;
        const float MinScale = 0.5f;

        // Lock(L2/Tab)ホールド中のアンカー追従。押した瞬間のフォーカスノードの画面位置を
        // アンカーとして記憶し、ホールド中はフォーカスノードがそこに来るように追従する。
        bool lockHeld;
        LayeredNode lockNode;
        Vector2 lockAnchor;

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

        public GraphContentTransformer(VisualElement mask, VisualElement content, GraphInputAction graphInputAction,
            GroupGuideView groupGuideView)
        {
            this.mask = mask;
            this.content = content;
            this.graphInputAction = graphInputAction;
            this.groupGuideView = groupGuideView;
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
            SetTranslation(ResetTranslation);
            content.style.scale = Vector3.one;
            // ロック中のリセットは、リセット後の位置を新しいアンカーとして追認する。
            // やらないと次のフォーカス移動でリセット前の画面位置へ巻き戻る。
            RederiveLockAnchor();
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

            // ロック中はマスク中心ズームで動いた先を新しいアンカーとして追認する
            // (次のフォーカス移動でズーム前の位置へスナップさせない)。
            RederiveLockAnchor();
        }

        /// <summary>ロック対象ノードの現在の画面位置でアンカーを取り直す。</summary>
        void RederiveLockAnchor()
        {
            if (lockNode != null)
            {
                lockAnchor = translation + lockNode.TargetPosition * currentScale;
            }
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
            // ロック中の手動パンはアンカーごと動かす(次のフォーカス移動と喧嘩させない)
            if (lockNode != null)
            {
                lockAnchor += delta;
            }
        }

        /// <summary>
        /// Lock(L2/Tab)ホールドの開始。nodeの現在の画面位置をアンカーとして記憶する。
        /// 中央へは寄せない(押した瞬間に画面が動かない)。フォーカスが無いまま押した場合は
        /// ホールドだけ有効にし、次にフォーカスされたノードがその場でアンカー化される。
        /// </summary>
        /// <remarks>アニメーション遷移中でも確定値(translationの目標値)基準で取る。</remarks>
        public void BeginLockFollow(LayeredNode node)
        {
            lockHeld = true;
            lockNode = node;
            RederiveLockAnchor();
        }

        public void EndLockFollow()
        {
            lockHeld = false;
            lockNode = null;
        }

        /// <summary>
        /// フォーカスの移動をロック追従へ伝える。ロック(L2/Tab)を握っていなければ何もしない。
        /// </summary>
        public void FollowLockedNode(LayeredNode node)
        {
            if (!lockHeld) return;

            if (lockNode == null)
            {
                // フォーカス無しでロックを握り、後からノードを選んだ。画面は動かさず
                // そのノードのいまの位置をアンカーにする
                lockNode = node;
                RederiveLockAnchor();
                return;
            }

            // 新しいフォーカスがアンカー位置に来るように追従する
            lockNode = node;
            SetTranslation(lockAnchor - node.TargetPosition * currentScale);
        }

        public void Dispose()
        {
            disposable.Dispose();
        }
    }
}
