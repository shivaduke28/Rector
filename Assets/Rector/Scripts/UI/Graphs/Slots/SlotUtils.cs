using System;
using R3;
using UnityEngine;

namespace Rector.UI.Graphs.Slots
{
    public static class SlotUtils
    {
        public static SlotValueType GetSlotValueType<T>()
        {
            var type = typeof(T);
            return type switch
            {
                not null when type == typeof(Unit) => SlotValueType.Unit,
                not null when type == typeof(int) => SlotValueType.Int,
                not null when type == typeof(float) => SlotValueType.Float,
                not null when type == typeof(bool) => SlotValueType.Boolean,
                not null when type == typeof(Texture) => SlotValueType.Texture,
                not null when type == typeof(Transform) => SlotValueType.Transform,
                not null when type == typeof(Vector3) => SlotValueType.Vector3,
                _ => throw new ArgumentOutOfRangeException($"{type.Name} can not be converted to SlotValueType")
            };
        }
    }
}
