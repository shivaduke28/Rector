using System;
using R3;
using UnityEngine.UIElements;

namespace Rector.UI.GraphPages
{
    /// <summary>
    /// グラフゾーン右下の操作ガイド。表記だけでなくレイアウトごと差し替わるので、
    /// パッド用(<see cref="PadGuideView"/>)とキーボード用(<see cref="KeyboardGuideView"/>)を
    /// 両方持って設定(<see cref="InputGuideMode"/>)で表示を切り替える。
    /// 中身は <see cref="InputGuideContents"/> の1つの表を両方が読む。
    /// 押しているボタンはチップの反転で見せる。押下は
    /// <see cref="GraphInputAction.Pressed"/> を位置ごとに購読して受け取る。
    /// </summary>
    public sealed class InputGuideView : VisualElement
    {
        static readonly GuideInput[] Inputs = (GuideInput[])Enum.GetValues(typeof(GuideInput));

        readonly PadGuideView padView = new();
        readonly KeyboardGuideView keyboardView = new();

        GraphPageState currentState;

        public InputGuideView()
        {
            AddToClassList(InputGuideClassNames.Root);
            pickingMode = PickingMode.Ignore;

            Add(padView);
            Add(keyboardView);
            keyboardView.style.display = DisplayStyle.None;
        }

        public IDisposable Bind(
            ReadOnlyReactiveProperty<GraphPageState> state,
            GraphInputAction input,
            ReadOnlyReactiveProperty<InputGuideMode> mode)
        {
            var disposables = new CompositeDisposable();

            state.Subscribe(x =>
            {
                currentState = x;
                UpdateContent();
            }).AddTo(disposables);

            mode.Subscribe(x =>
            {
                style.display = x == InputGuideMode.Off ? DisplayStyle.None : DisplayStyle.Flex;

                var keyboard = x == InputGuideMode.Keyboard;
                padView.style.display = keyboard ? DisplayStyle.None : DisplayStyle.Flex;
                keyboardView.style.display = keyboard ? DisplayStyle.Flex : DisplayStyle.None;
                padView.SetXbox(x == InputGuideMode.Xbox);

                UpdateContent();
            }).AddTo(disposables);

            // 隠れている方にも流す。切り替え時に古い反転が残る分岐を持たない方が安い
            foreach (var guideInput in Inputs)
            {
                var captured = guideInput;
                input.Pressed(captured).Subscribe(pressed =>
                {
                    padView.SetPressed(captured, pressed);
                    keyboardView.SetPressed(captured, pressed);
                }).AddTo(disposables);
            }

            return disposables;
        }

        void UpdateContent()
        {
            var content = InputGuideContents.Get(currentState);
            padView.Apply(content);
            keyboardView.Apply(content);
        }
    }
}
