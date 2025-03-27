using System;
using R3;
using Rector.UI.Graphs;
using UnityEngine;

namespace Rector.UI.Nodes
{
    public enum SlotDirection
    {
        Input,
        Output,
    }

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

    public enum SlotValueType
    {
        Unit,
        Boolean,
        Float,
        Int,
        Texture,
        Transform,
        Vector3,
    }

    public interface ISlot
    {
        NodeId NodeId { get; }
        SlotDirection Direction { get; }
        string Name { get; }
        ReactiveProperty<bool> Selected { get; }
        SlotValueType Type { get; }
        int Index { get; }
        int ConnectedCount { get; }
        void OnConnected();
        void Disconnected();
    }

    public abstract class InputSlot : ISlot
    {
        public NodeId NodeId { get; }
        public int Index { get; }
        public SlotValueType Type { get; }
        public SlotDirection Direction => SlotDirection.Input;
        public string Name { get; }
        public ReactiveProperty<bool> Selected { get; } = new(false);
        public int ConnectedCount { get; private set; }

        public virtual void OnConnected()
        {
            ConnectedCount++;
        }

        public virtual void Disconnected()
        {
            ConnectedCount--;
        }

        protected InputSlot(NodeId nodeId, int index, SlotValueType type, string name)
        {
            NodeId = nodeId;
            Index = index;
            Name = name;
            Type = type;
        }
    }

    public abstract class InputSlot<T> : InputSlot
    {
        public abstract void Send(T value);

        public abstract Observable<T> Observable();

        protected InputSlot(NodeId nodeId, int index, string name) : base(nodeId, index,
            SlotUtils.GetSlotValueType<T>(), name)
        {
        }
    }

    public sealed class CallbackInputSlot : InputSlot<Unit>
    {
        readonly Subject<Unit> subject = new();
        readonly Action action;
        readonly ReadOnlyReactiveProperty<bool> isMuted;

        public CallbackInputSlot(NodeId nodeId, int index, string name, Action action, ReadOnlyReactiveProperty<bool> isMuted) : base(nodeId, index, name)
        {
            this.action = action;
            this.isMuted = isMuted;
        }

        public void SendForce()
        {
            action.Invoke();
            subject.OnNext(Unit.Default);
        }

        public override void Send(Unit value)
        {
            if (isMuted.CurrentValue) return;
            action.Invoke();
            subject.OnNext(value);
        }

        public override Observable<Unit> Observable() => subject;
    }

#nullable enable
    // abstractにしてValueTypeごとに作った方がいいかもしれん
    public class ReactivePropertyInputSlot<T> : InputSlot<T>
    {
        // Muteを見ていないのでグラフからは使わないようにする
        public readonly ReactiveProperty<T> Property;
        readonly T? defaultValue;
        readonly ReadOnlyReactiveProperty<bool> isMuted;
        public override void Send(T value)
        {
            if (isMuted.CurrentValue) return;
            Property.Value = value;
        }

        public override Observable<T> Observable() => Property;

        public override void Disconnected()
        {
            base.Disconnected();
            if (ConnectedCount == 0 && defaultValue != null)
            {
                Property.Value = defaultValue;
            }
        }

        public ReactivePropertyInputSlot(NodeId nodeId, int index, string name, ReactiveProperty<T> property, T defaultValue, ReadOnlyReactiveProperty<bool> isMuted) : base(
            nodeId, index, name)
        {
            Property = property;
            this.defaultValue = defaultValue;
            this.isMuted = isMuted;
        }
    }
#nullable disable

    public sealed class ReactivePropertyFloatInputSlot : ReactivePropertyInputSlot<float>
    {
        public readonly float MinValue;
        public readonly float MaxValue;

        public ReactivePropertyFloatInputSlot(NodeId nodeId, int index, string name, ReactiveProperty<float> property,
            float defaultValue, float minValue, float maxValue, ReadOnlyReactiveProperty<bool> isMuted) : base(nodeId, index, name, property, defaultValue, isMuted)
        {
            MinValue = minValue;
            MaxValue = maxValue;
        }
    }

    public sealed class ReactivePropertyIntInputSlot : ReactivePropertyInputSlot<int>
    {
        public readonly int MinValue;
        public readonly int MaxValue;

        public ReactivePropertyIntInputSlot(NodeId nodeId, int index, string name, ReactiveProperty<int> property,
            int defaultValue, int minValue, int maxValue, ReadOnlyReactiveProperty<bool> isMuted) : base(nodeId, index, name, property, defaultValue, isMuted)
        {
            MinValue = minValue;
            MaxValue = maxValue;
        }
    }

    public abstract class OutputSlot : ISlot
    {
        public NodeId NodeId { get; }
        public int Index { get; }
        public int ConnectedCount { get; private set; }
        public void OnConnected() => ConnectedCount++;

        public void Disconnected() => ConnectedCount--;

        public SlotValueType Type { get; }
        public SlotDirection Direction => SlotDirection.Output;
        public string Name { get; }
        public ReactiveProperty<bool> Selected { get; } = new(false);

        protected OutputSlot(NodeId nodeId, int index, SlotValueType type, string name)
        {
            NodeId = nodeId;
            Index = index;
            Name = name;
            Type = type;
        }
    }

    public abstract class OutputSlot<T> : OutputSlot
    {
        public abstract Observable<T> Observable();

        protected OutputSlot(NodeId nodeId, int index, string name) : base(nodeId, index,
            SlotUtils.GetSlotValueType<T>(), name)
        {
        }
    }

    public sealed class ObservableOutputSlot<T> : OutputSlot<T>
    {
        readonly Observable<T> observable;
        readonly ReadOnlyReactiveProperty<bool> isMuted;

        public override Observable<T> Observable() => observable.Where(_ => !isMuted.CurrentValue);

        public ObservableOutputSlot(NodeId nodeId, int index, string name, Observable<T> observable, ReadOnlyReactiveProperty<bool> isMuted) : base(nodeId,
            index, name)
        {
            this.observable = observable;
            this.isMuted = isMuted;
        }
    }
}
