using System.Collections.Generic;
using NUnit.Framework;
using Rector.UI.Settings;

namespace Rector.Tests.EditMode
{
    /// <summary>
    /// 設定行の状態遷移のテスト。
    /// ステッパーは端でclampして値を飛ばさないこと、
    /// セレクターはメニューを閉じるまで値を適用しないことが要。
    /// </summary>
    public sealed class SettingRowStateTests
    {
        static readonly string[] Three = { "a", "b", "c" };

        [Test]
        public void Stepper_ClampsAtBothEnds()
        {
            var changed = new List<int>();
            var stepper = new StepperRowState("label", Three, 0, changed.Add);
            ISettingRow row = stepper;

            row.OnHorizontal(-1);
            Assert.AreEqual(0, stepper.SelectedIndex.CurrentValue);
            CollectionAssert.IsEmpty(changed);

            row.OnHorizontal(1);
            row.OnHorizontal(1);
            row.OnHorizontal(1);
            Assert.AreEqual(2, stepper.SelectedIndex.CurrentValue);
            CollectionAssert.AreEqual(new[] { 1, 2 }, changed);
        }

        [Test]
        public void Stepper_NeverCapturesInput()
        {
            ISettingRow row = new StepperRowState("label", Three, 0, _ => { });

            row.OnSubmit();
            Assert.IsFalse(row.IsCapturingInput.CurrentValue);
        }

        [Test]
        public void Stepper_SetIndexWithoutNotify_DoesNotInvokeCallback()
        {
            var changed = new List<int>();
            var stepper = new StepperRowState("label", Three, 0, changed.Add);

            stepper.SetIndexWithoutNotify(2);

            Assert.AreEqual(2, stepper.SelectedIndex.CurrentValue);
            CollectionAssert.IsEmpty(changed);
        }

        [Test]
        public void Selector_CommitsOnlyOnSubmitInsideMenu()
        {
            var committed = new List<int>();
            var selector = new SelectorRowState("label", committed.Add);
            selector.SetOptions(Three, 0);
            ISettingRow row = selector;

            // 閉じている間は左右も上下も値を動かさない
            row.OnHorizontal(1);
            row.OnVertical(1);
            Assert.AreEqual(0, selector.SelectedIndex.CurrentValue);
            CollectionAssert.IsEmpty(committed);

            row.OnSubmit();
            Assert.IsTrue(selector.IsExpanded.CurrentValue);
            Assert.IsTrue(row.IsCapturingInput.CurrentValue);

            row.OnVertical(1);
            Assert.AreEqual(1, selector.CursorIndex.CurrentValue);
            Assert.AreEqual(0, selector.SelectedIndex.CurrentValue, "確定するまで値は動かない");
            CollectionAssert.IsEmpty(committed);

            row.OnSubmit();
            Assert.IsFalse(selector.IsExpanded.CurrentValue);
            Assert.AreEqual(1, selector.SelectedIndex.CurrentValue);
            CollectionAssert.AreEqual(new[] { 1 }, committed);
        }

        [Test]
        public void Selector_CancelDiscardsCursor()
        {
            var committed = new List<int>();
            var selector = new SelectorRowState("label", committed.Add);
            selector.SetOptions(Three, 0);
            ISettingRow row = selector;

            row.OnSubmit();
            row.OnVertical(1);
            row.OnCancel();

            Assert.IsFalse(selector.IsExpanded.CurrentValue);
            Assert.AreEqual(0, selector.SelectedIndex.CurrentValue);
            CollectionAssert.IsEmpty(committed);

            // 開き直したらカーソルは現在値に戻る
            row.OnSubmit();
            Assert.AreEqual(0, selector.CursorIndex.CurrentValue);
        }

        [Test]
        public void Selector_CursorWrapsAround()
        {
            var selector = new SelectorRowState("label", _ => { });
            selector.SetOptions(Three, 0);
            ISettingRow row = selector;

            row.OnSubmit();
            row.OnVertical(-1);
            Assert.AreEqual(2, selector.CursorIndex.CurrentValue);

            row.OnVertical(1);
            Assert.AreEqual(0, selector.CursorIndex.CurrentValue);
        }

        [Test]
        public void Selector_ReSelectingCurrentValueDoesNotCommit()
        {
            var committed = new List<int>();
            var selector = new SelectorRowState("label", committed.Add);
            selector.SetOptions(Three, 1);
            ISettingRow row = selector;

            row.OnSubmit();
            row.OnSubmit();

            Assert.IsFalse(selector.IsExpanded.CurrentValue);
            Assert.AreEqual(1, selector.SelectedIndex.CurrentValue);
            CollectionAssert.IsEmpty(committed);
        }

        [Test]
        public void Selector_WithoutOptions_DoesNotExpand()
        {
            var selector = new SelectorRowState("label", _ => { });
            ISettingRow row = selector;

            row.OnSubmit();

            Assert.IsFalse(selector.IsExpanded.CurrentValue);
            Assert.AreEqual(-1, selector.SelectedIndex.CurrentValue);
        }

        [Test]
        public void Selector_SetOptions_KeepsUnselectedAsUnselected()
        {
            var selector = new SelectorRowState("label", _ => { });

            // 現在値が候補に無いときに先頭へ丸めると、選んでいない値を現在値として見せてしまう
            selector.SetOptions(Three, -1);

            Assert.AreEqual(-1, selector.SelectedIndex.CurrentValue);
            Assert.AreEqual(0, selector.CursorIndex.CurrentValue);
        }

        [Test]
        public void Selector_SetOptions_ClosesMenuAndClampsSelection()
        {
            var selector = new SelectorRowState("label", _ => { });
            selector.SetOptions(Three, 2);
            ISettingRow row = selector;

            row.OnSubmit();
            Assert.IsTrue(selector.IsExpanded.CurrentValue);

            selector.SetOptions(new[] { "x", "y" }, 5);

            Assert.IsFalse(selector.IsExpanded.CurrentValue);
            Assert.AreEqual(1, selector.SelectedIndex.CurrentValue);
            Assert.AreEqual(1, selector.CursorIndex.CurrentValue);
        }

        [Test]
        public void Text_ShowsWhatItIsToldAndSwallowsEveryInput()
        {
            var text = new TextRowState("label", "before");
            ISettingRow row = text;

            row.OnHorizontal(1);
            row.OnVertical(1);
            row.OnSubmit();
            row.OnCancel();

            Assert.AreEqual("before", text.Value.CurrentValue);
            // 占有しないので、ページのカーソル移動と「閉じる」は行に奪われない
            Assert.IsFalse(row.IsCapturingInput.CurrentValue);

            text.SetText("after");
            Assert.AreEqual("after", text.Value.CurrentValue);
        }
    }
}
