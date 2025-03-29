using R3;
using UnityEngine;

namespace Rector.UI.GraphPages
{
    public sealed class HoldGuideModel
    {
        public readonly ReactiveProperty<Vector2> Position = new(Vector2.zero);
        public readonly ReactiveProperty<bool> Visible = new(false);
    }
}
