using System.Collections.Generic;
using R3;

namespace Rector.UI.Hud
{
    public interface IButtonListPageModel
    {
        void Submit();
        void Cancel();
        void Navigate(bool up);
        IEnumerable<RectorButtonState> GetButtons();
        ReadOnlyReactiveProperty<bool> IsVisible { get; }
    }
}
