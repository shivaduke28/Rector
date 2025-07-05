using System;
using System.Linq;
using R3;
using Rector.UI.Graphs.Nodes;
using Rector.UI.Graphs.Slots;
using UnityEngine;

namespace Rector.UI.Graphs
{
    // FIXME: 型ベースの接続可能性とグラフ構造による接続可能性は別の話なのでこのクラスはGraphsに移動してcycleの検知を別のクラスとして作る
    public static class EdgeConnector
    {
        public static bool CanConnect(ISlot slot, Node node)
        {
            return slot.Direction == SlotDirection.Input
                ? node.OutputSlots.Any(x => CanConnect(slot, x))
                : node.InputSlots.Any(x => CanConnect(slot, x));
        }

        public static bool CanConnect(ISlot slot1, ISlot slot2)
        {
            if (slot1.Direction == slot2.Direction) return false;
            if (slot1.NodeId.Equals(slot2.NodeId)) return false;
            return CanConnect(slot1.Type, slot2.Type);
        }

        static bool CanConnect(SlotValueType output, SlotValueType input)
        {
            return output switch
            {
                SlotValueType.Unit => input is SlotValueType.Unit or SlotValueType.Boolean or SlotValueType.Float or SlotValueType.Int,
                SlotValueType.Boolean => input is SlotValueType.Unit or SlotValueType.Boolean or SlotValueType.Float or SlotValueType.Int,
                SlotValueType.Float => input is SlotValueType.Unit or SlotValueType.Boolean or SlotValueType.Float or SlotValueType.Int,
                SlotValueType.Int => input is SlotValueType.Unit or SlotValueType.Boolean or SlotValueType.Float or SlotValueType.Int,
                SlotValueType.Texture => input == SlotValueType.Texture,
                SlotValueType.Transform => input == SlotValueType.Transform,
                SlotValueType.Vector3 => input == SlotValueType.Vector3,
                _ => output == input
            };
        }

        public static bool TryConnect(ISlot slot1, ISlot slot2, out Edge edge)
        {
            edge = null;
            if (!CanConnect(slot1, slot2)) return false;
            var output = slot1.Direction == SlotDirection.Output ? (OutputSlot)slot1 : (OutputSlot)slot2;
            var input = slot2.Direction == SlotDirection.Input ? (InputSlot)slot2 : (InputSlot)slot1;
            IDisposable disposable;
            switch (output.Type)
            {
                case SlotValueType.Unit:
                    var outputUnit = (OutputSlot<Unit>)output;
                    disposable = input.Type switch
                    {
                        SlotValueType.Unit => outputUnit.Observable().Subscribe(((InputSlot<Unit>)input).Send),
                        SlotValueType.Boolean => outputUnit.Observable().Select(_ => true).Subscribe(((InputSlot<bool>)input).Send),
                        SlotValueType.Float => outputUnit.Observable().Select(_ => 1f).Subscribe(((InputSlot<float>)input).Send),
                        SlotValueType.Int => outputUnit.Observable().Select(_ => 1).Subscribe(((InputSlot<int>)input).Send),
                        _ => throw new ArgumentOutOfRangeException()
                    };
                    break;
                case SlotValueType.Boolean:
                    var outputBool = (OutputSlot<bool>)output;
                    disposable = input.Type switch
                    {
                        SlotValueType.Unit =>
                            // TODO: trueの場合だけ通知で良いのか考える
                            outputBool.Observable().Where(x => x).AsUnitObservable().Subscribe(((InputSlot<Unit>)input).Send),
                        SlotValueType.Boolean => outputBool.Observable().Subscribe(((InputSlot<bool>)input).Send),
                        SlotValueType.Float => outputBool.Observable().Select(x => x ? 1f : 0f).Subscribe(((InputSlot<float>)input).Send),
                        SlotValueType.Int => outputBool.Observable().Select(x => x ? 1 : 0).Subscribe(((InputSlot<int>)input).Send),
                        _ => throw new ArgumentOutOfRangeException()
                    };
                    break;
                case SlotValueType.Float:
                    var outputFloat = (OutputSlot<float>)output;
                    disposable = input.Type switch
                    {
                        SlotValueType.Unit => outputFloat.Observable().Where(x => x != 0).AsUnitObservable().Subscribe(((InputSlot<Unit>)input).Send),
                        SlotValueType.Boolean => outputFloat.Observable().Select(x => x != 0).Subscribe(((InputSlot<bool>)input).Send),
                        SlotValueType.Float => outputFloat.Observable().Subscribe(((InputSlot<float>)input).Send),
                        SlotValueType.Int => outputFloat.Observable().Select(x => (int)x).Subscribe(((InputSlot<int>)input).Send),
                        _ => throw new ArgumentOutOfRangeException()
                    };
                    break;
                case SlotValueType.Int:
                    var outputInt = (OutputSlot<int>)output;
                    disposable = input.Type switch
                    {
                        SlotValueType.Unit => outputInt.Observable().Where(x => x != 0).AsUnitObservable().Subscribe(((InputSlot<Unit>)input).Send),
                        SlotValueType.Boolean => outputInt.Observable().Select(x => x != 0).Subscribe(((InputSlot<bool>)input).Send),
                        SlotValueType.Float => outputInt.Observable().Select(x => (float)x).Subscribe(((InputSlot<float>)input).Send),
                        SlotValueType.Int => outputInt.Observable().Subscribe(((InputSlot<int>)input).Send),
                        _ => throw new ArgumentOutOfRangeException()
                    };
                    break;
                case SlotValueType.Texture:
                    var outputTexture = (OutputSlot<Texture>)output;
                    disposable = input.Type switch
                    {
                        SlotValueType.Texture => outputTexture.Observable().Subscribe(((InputSlot<Texture>)input).Send),
                        _ => throw new ArgumentOutOfRangeException()
                    };
                    break;
                case SlotValueType.Transform:
                    var outputTransform = (OutputSlot<Transform>)output;
                    disposable = input.Type switch
                    {
                        SlotValueType.Transform => outputTransform.Observable().Subscribe(((InputSlot<Transform>)input).Send),
                        _ => throw new ArgumentOutOfRangeException()
                    };
                    break;
                case SlotValueType.Vector3:
                    var outputVector3 = (OutputSlot<Vector3>)output;
                    disposable = input.Type switch
                    {
                        SlotValueType.Vector3 => outputVector3.Observable().Subscribe(((InputSlot<Vector3>)input).Send),
                        _ => throw new ArgumentOutOfRangeException()
                    };
                    break;
                default:
                    return false;
            }
            edge = new Edge(output, input, disposable);
            return true;
        }
    }
}
