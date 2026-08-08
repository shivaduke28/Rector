using System;
using System.Collections.Generic;
using System.Linq;
using R3;
using Rector.UI.Settings;
using UnityEngine;

namespace Rector.UI.Hud
{
    public sealed class DisplaySettingsPageModel : IInitializable, IDisposable, ISettingsPageModel
    {
        static readonly FullScreenMode[] FullScreenModes =
        {
            FullScreenMode.ExclusiveFullScreen,
            FullScreenMode.FullScreenWindow,
            FullScreenMode.MaximizedWindow,
            FullScreenMode.Windowed,
        };

        readonly SettingsPageView view;
        readonly ReactiveProperty<bool> isVisible = new(false);
        ReadOnlyReactiveProperty<bool> ISettingsPageModel.IsVisible => isVisible;

        // どちらも送るたびにウィンドウが跳ねるので、メニューを閉じるまで適用しない行にする
        readonly SelectorRowState fullScreenRow;
        readonly SelectorRowState resolutionRow;
        readonly ISettingRow[] rows;

        /// <summary>解像度行の候補。表示文字列と対で、確定したインデックスから引く。</summary>
        readonly List<Vector2Int> resolutions = new();

        Action onExit;
        IDisposable disposable;

        public DisplaySettingsPageModel(SettingsPageView view)
        {
            this.view = view;

            fullScreenRow = new SelectorRowState("Screen Mode", i => ChangeFullScreenMode(FullScreenModes[i]));
            resolutionRow = new SelectorRowState("Resolution", i => UpdateResolution(resolutions[i]));
            rows = new ISettingRow[] { fullScreenRow, resolutionRow };
        }

        public void Initialize() => disposable = view.Bind(this);

        public void Dispose() => disposable?.Dispose();

        public void Enter(Action onExitAction)
        {
            onExit = onExitAction;

            // 現在値は毎回Screenから読み直す。ここ以外から変わっていても表示が嘘にならない。
            fullScreenRow.SetOptions(
                FullScreenModes.Select(x => x.ToString()).ToArray(),
                Array.IndexOf(FullScreenModes, Screen.fullScreenMode));
            RefreshResolutions();

            isVisible.Value = true;
        }

        /// <summary>
        /// 環境の解像度を読み直す。Screen.resolutionsはリフレッシュレート違いを別々に返すが、
        /// 適用時のリフレッシュレートは60固定なので、幅と高さで重複を潰して1行にする。
        /// </summary>
        void RefreshResolutions()
        {
            resolutions.Clear();
            resolutions.AddRange(Screen.resolutions
                .Select(x => new Vector2Int(x.width, x.height))
                .Distinct()
                .OrderBy(x => x.x)
                .ThenBy(x => x.y));

            // ウィンドウモードでは今のサイズが候補に無いことがある。
            // 無いまま別の候補を現在値として見せると嘘になるので、先頭に足して選ばせる。
            var current = new Vector2Int(Screen.width, Screen.height);
            var index = resolutions.IndexOf(current);
            if (index < 0)
            {
                resolutions.Insert(0, current);
                index = 0;
            }

            resolutionRow.SetOptions(resolutions.Select(ToLabel).ToArray(), index);
        }

        IReadOnlyList<ISettingRow> ISettingsPageModel.GetRows() => rows;

        void ISettingsPageModel.Cancel()
        {
            isVisible.Value = false;
            onExit?.Invoke();
            onExit = null;
        }

        static string ToLabel(Vector2Int resolution) => $"{resolution.x} x {resolution.y}";

        static void ChangeFullScreenMode(FullScreenMode fullScreenMode)
        {
            Screen.SetResolution(Screen.width, Screen.height, fullScreenMode, new RefreshRate
            {
                numerator = 60,
                denominator = 1
            });
            RectorLogger.Resolution(Screen.width, Screen.height, fullScreenMode);
        }

        static void UpdateResolution(Vector2Int resolution)
        {
            Screen.SetResolution(resolution.x, resolution.y, Screen.fullScreenMode, new RefreshRate
            {
                numerator = 60,
                denominator = 1
            });
            RectorLogger.Resolution(resolution.x, resolution.y, Screen.fullScreenMode);
        }
    }
}
