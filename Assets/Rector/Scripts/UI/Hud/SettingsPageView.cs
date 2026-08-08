using System;
using System.Collections.Generic;
using R3;
using Rector.UI.Settings;
using UnityEngine;
using UnityEngine.UIElements;

namespace Rector.UI.Hud
{
    public interface ISettingsPageModel
    {
        IReadOnlyList<ISettingRow> GetRows();
        ReadOnlyReactiveProperty<bool> IsVisible { get; }

        /// <summary>ページを閉じる。行がメニューを開いている間は呼ばれない。</summary>
        void Cancel();
    }

    /// <summary>
    /// 1設定1行の設定ページ。行カーソルと入力の振り分けだけを持ち、
    /// 値の持ち方と反映のタイミングは行(<see cref="ISettingRow"/>)に任せる。
    /// </summary>
    /// <remarks>
    /// 「項目を選んで決定する」ページは<see cref="ButtonListPageView"/>のまま。
    /// こちらは行の上で値を変える形なので、Submitの意味が行ごとに違う。
    /// </remarks>
    public sealed class SettingsPageView : IUIInputHandler
    {
        const string MenuLayerClassName = "rector-setting-menu-layer";

        readonly VisualElement root;
        readonly UIInputAction uiInputAction;
        readonly VisualElement settingList;
        readonly SerialDisposable inputDisposable = new();

        ISettingsPageModel model;
        IReadOnlyList<ISettingRow> rows = Array.Empty<ISettingRow>();
        int index;

        public SettingsPageView(VisualElement root, UIInputAction uiInputAction)
        {
            this.root = root;
            this.uiInputAction = uiInputAction;
            settingList = root.Q<VisualElement>("setting-list");
        }

        public IDisposable Bind(ISettingsPageModel page)
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

            settingList.Clear();
            rows = model.GetRows();
            index = 0;

            // UI Toolkitでは階層の後ろにある要素ほど手前に描かれる。展開したメニューが
            // 下の行に潜らないよう、行を全部並べたあとに専用レイヤーを重ねる。
            var menuLayer = new VisualElement { pickingMode = PickingMode.Ignore };
            menuLayer.AddToClassList(MenuLayerClassName);

            var d = new CompositeDisposable();
            for (var i = 0; i < rows.Count; i++)
            {
                var row = rows[i];
                row.IsFocused.Value = i == index;
                settingList.Add(CreateElement(row, menuLayer, d));
            }

            settingList.Add(menuLayer);
            inputDisposable.Disposable = d;
        }

        void Hide()
        {
            uiInputAction.Unregister(this);
            root.style.display = DisplayStyle.None;
            inputDisposable.Disposable = null;

            foreach (var row in rows)
            {
                row.IsFocused.Value = false;
            }
        }

        static VisualElement CreateElement(ISettingRow row, VisualElement menuLayer, CompositeDisposable disposable)
        {
            switch (row)
            {
                case StepperRowState stepper:
                    {
                        var element = new RectorSettingStepper();
                        element.Bind(stepper).AddTo(disposable);
                        return element;
                    }
                case SelectorRowState selector:
                    {
                        var element = new RectorSettingSelector();
                        element.Bind(selector, menuLayer).AddTo(disposable);
                        return element;
                    }
                default:
                    throw new ArgumentOutOfRangeException(nameof(row), row, "unknown setting row");
            }
        }

        void IUIInputHandler.OnNavigate(Vector2 value)
        {
            if (rows.Count == 0) return;

            // NavigateInputThrottleは優勢な1方向しか流さないので、斜めは来ない
            var row = rows[index];
            if (value.y != 0)
            {
                var delta = value.y > 0 ? -1 : 1;
                if (row.IsCapturingInput.CurrentValue)
                {
                    row.OnVertical(delta);
                }
                else
                {
                    MoveCursor(delta);
                }
            }
            else if (value.x != 0)
            {
                if (row.IsCapturingInput.CurrentValue) return;

                row.OnHorizontal(value.x > 0 ? 1 : -1);
            }
        }

        void MoveCursor(int delta)
        {
            rows[index].IsFocused.Value = false;
            index = (index + delta + rows.Count) % rows.Count;
            rows[index].IsFocused.Value = true;
        }

        void IUIInputHandler.OnSubmit()
        {
            if (rows.Count == 0) return;

            rows[index].OnSubmit();
        }

        void IUIInputHandler.OnCancel()
        {
            if (rows.Count > 0 && rows[index].IsCapturingInput.CurrentValue)
            {
                rows[index].OnCancel();
                return;
            }

            model.Cancel();
        }
    }
}
