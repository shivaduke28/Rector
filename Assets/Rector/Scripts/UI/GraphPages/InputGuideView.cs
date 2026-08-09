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
    /// </summary>
    public sealed class InputGuideView : VisualElement
    {
        readonly PadGuideView padView = new();
        readonly KeyboardGuideView keyboardView = new();

        GraphPageState currentState;
        bool grabHeld;
        bool lockHeld;

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
            ReadOnlyReactiveProperty<bool> grabModifierHeld,
            ReadOnlyReactiveProperty<bool> lockHeldProperty,
            ReadOnlyReactiveProperty<InputGuideMode> mode)
        {
            return new CompositeDisposable(
                state.Subscribe(x =>
                {
                    currentState = x;
                    UpdateContent();
                }),
                grabModifierHeld.Subscribe(x =>
                {
                    grabHeld = x;
                    UpdateContent();
                }),
                lockHeldProperty.Subscribe(x =>
                {
                    lockHeld = x;
                    UpdateContent();
                }),
                mode.Subscribe(x =>
                {
                    style.display = x == InputGuideMode.Off ? DisplayStyle.None : DisplayStyle.Flex;

                    var keyboard = x == InputGuideMode.Keyboard;
                    padView.style.display = keyboard ? DisplayStyle.None : DisplayStyle.Flex;
                    keyboardView.style.display = keyboard ? DisplayStyle.Flex : DisplayStyle.None;
                    padView.SetXbox(x == InputGuideMode.Xbox);

                    UpdateContent();
                })
            );
        }

        // 隠れている方も一緒に更新する。ラベル代入が十数個増えるだけで、
        // 切り替え時に古い表示が残る分岐を持たない方が安い。
        void UpdateContent()
        {
            var content = InputGuideContents.Get(currentState);
            padView.Apply(content, grabHeld, lockHeld);
            keyboardView.Apply(content, grabHeld, lockHeld);
        }
    }
}
