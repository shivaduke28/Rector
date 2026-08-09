using System;
using System.Collections.Generic;
using System.Linq;
using R3;
using UnityEngine;
using UnityEngine.UIElements;

namespace Rector.UI.Hud
{
    /// <summary>
    /// ボタン一覧の1行。見出しかボタンのどちらか。
    /// </summary>
    /// <remarks>
    /// 見出しと余白はカーソルが止まらない飾りなので、モデルは行の並び(<see cref="IButtonListPageModel.GetItems"/>)と
    /// カーソルの対象(ボタンだけの配列)を別々に持つ。飾りを飛ばす処理はどこにも要らない。
    /// </remarks>
    public readonly struct ButtonListItem
    {
        /// <summary>見出しのテキスト。ボタンと余白の行では null。</summary>
        public readonly string HeaderText;

        /// <summary>ボタンの状態。見出しと余白の行では null。</summary>
        public readonly RectorButtonState Button;

        public bool IsHeader => Button == null;

        /// <summary>文字を持たない見出し。行のまとまりを空きだけで分ける。</summary>
        public bool IsSpacer => Button == null && HeaderText == null;

        ButtonListItem(string headerText, RectorButtonState button)
        {
            HeaderText = headerText;
            Button = button;
        }

        public static ButtonListItem Of(RectorButtonState button) => new(null, button);

        public static ButtonListItem Header(string text) => new(text, null);

        public static ButtonListItem Spacer() => new(null, null);
    }

    public interface IButtonListPageModel
    {
        void Submit();
        void Cancel();
        void Navigate(bool up);

        /// <summary>上から並べる順の行。見出しを混ぜてよい。</summary>
        IEnumerable<ButtonListItem> GetItems();

        ReadOnlyReactiveProperty<bool> IsVisible { get; }
    }

    public sealed class ButtonListPageView : IUIInputHandler
    {
        const string HeaderClassName = "rector-button-list-header";
        const string SpacerClassName = "rector-button-list-spacer";

        readonly VisualElement root;
        readonly UIInputAction uiInputAction;
        readonly VisualElement leftList;
        readonly SerialDisposable inputDisposable = new();

        IButtonListPageModel model;

        public ButtonListPageView(VisualElement root, UIInputAction uiInputAction)
        {
            this.root = root;
            this.uiInputAction = uiInputAction;
            leftList = root.Q<VisualElement>("left-list");
        }

        public IDisposable Bind(IButtonListPageModel page)
        {
            model = page;
            var disposable = new CompositeDisposable();

            page.IsVisible.Subscribe(visible =>
            {
                if (visible)
                    Show();
                else
                    Hide();
            }).AddTo(disposable);
            inputDisposable.AddTo(disposable);
            return disposable;
        }

        void Show()
        {
            root.style.display = DisplayStyle.Flex;
            uiInputAction.Register(this);

            leftList.Clear();
            var d = new CompositeDisposable();
            foreach (var item in model.GetItems())
            {
                if (item.IsSpacer)
                {
                    var spacer = new VisualElement();
                    spacer.AddToClassList(SpacerClassName);
                    leftList.Add(spacer);
                    continue;
                }

                if (item.IsHeader)
                {
                    var header = new Label(item.HeaderText);
                    header.AddToClassList(HeaderClassName);
                    leftList.Add(header);
                    continue;
                }

                var rectorButton = new RectorButton();
                rectorButton.Bind(item.Button).AddTo(d);
                leftList.Add(rectorButton);
            }

            inputDisposable.Disposable = d;
        }

        void Hide()
        {
            uiInputAction.Unregister(this);
            root.style.display = DisplayStyle.None;
            inputDisposable.Disposable = null;
        }

        void IUIInputHandler.OnNavigate(Vector2 value)
        {
            if (value.y > 0)
            {
                model.Navigate(false);
            }
            else if (value.y < 0)
            {
                model.Navigate(true);
            }
        }

        void IUIInputHandler.OnSubmit()
        {
            model.Submit();
        }

        void IUIInputHandler.OnCancel()
        {
            model.Cancel();
        }
    }
}
