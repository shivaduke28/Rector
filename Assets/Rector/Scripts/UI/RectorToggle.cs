using System;
using R3;
using UnityEngine.UIElements;

namespace Rector.UI
{
    [UxmlElement("RectorToggle")]
    public sealed partial class RectorToggle : VisualElement
    {
        readonly Toggle toggle = new();
        const string UssClassName = "rector-toggle";

        [UxmlAttribute]
        public bool Value
        {
            get => toggle.value;
            set => toggle.value = value;
        }


        public RectorToggle()
        {
            AddToClassList(UssClassName);
            Add(toggle);
        }

        public IDisposable Bind(RectorToggleState state)
        {
            return new CompositeDisposable(
                state.Value.Subscribe(x => toggle.SetValueWithoutNotify(x)),
                toggle.OnValueChangeAsObservable().Subscribe(x => state.Value.Value = x.newValue)
            );
        }
    }
}
