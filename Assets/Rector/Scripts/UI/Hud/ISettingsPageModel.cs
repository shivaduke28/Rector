using System.Collections.Generic;
using R3;
using Rector.UI.Settings;

namespace Rector.UI.Hud
{
    public interface ISettingsPageModel
    {
        IReadOnlyList<ISettingRow> GetRows();
        ReadOnlyReactiveProperty<bool> IsVisible { get; }

        /// <summary>ページを閉じる。行がメニューを開いている間は呼ばれない。</summary>
        void Cancel();
    }
}
