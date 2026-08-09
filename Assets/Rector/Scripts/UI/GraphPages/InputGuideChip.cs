using UnityEngine.UIElements;

namespace Rector.UI.GraphPages
{
    /// <summary>
    /// ガイドの1項目。「キー名(ボタン名)」と「動作」を同じ色の下地に分けて載せ、キー名だけ枠で囲む。
    /// 押すものが四角として立つので、文字の並びから目で拾えるようになる。
    /// 空きは動作を「-」にして減光し、何も無いことを見せる。
    /// </summary>
    public sealed class InputGuideChip : VisualElement
    {
        readonly Label key;
        readonly Label action;

        public InputGuideChip(string keyText = "", string actionText = "")
        {
            AddToClassList(InputGuideClassNames.Chip);
            pickingMode = PickingMode.Ignore;

            key = new Label(keyText) { pickingMode = PickingMode.Ignore };
            key.AddToClassList(InputGuideClassNames.Key);
            Add(key);

            action = new Label(actionText) { pickingMode = PickingMode.Ignore };
            action.AddToClassList(InputGuideClassNames.Action);
            Add(action);
        }

        public void SetKey(string value) => key.text = value;

        /// <summary>
        /// キー名を枠で囲むか。△□◯✕のような記号はそれ自体がボタンの形なので、
        /// 枠を足すと二重になる。文字で呼ぶボタン(L2, Y, TAB)だけ囲む。
        /// </summary>
        public void SetKeyFramed(bool framed) => key.EnableInClassList(InputGuideClassNames.KeyPlain, !framed);

        public void SetAction(string value) => action.text = value ?? "-";

        public void SetState(bool enabled, bool active)
        {
            EnableInClassList(InputGuideClassNames.ChipDisabled, !enabled);
            EnableInClassList(InputGuideClassNames.ChipActive, enabled && active);
        }
    }
}
