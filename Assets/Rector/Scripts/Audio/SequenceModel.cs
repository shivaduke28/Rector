using System;
using R3;
using UnityEngine;

namespace Rector.Audio
{
    public sealed class SequenceModel : IDisposable
    {
        public const int MinLength = 1;
        public const int MaxLength = 256;
        public const int DefaultLength = 64;

        // length=1 でも毎Stepで拍イベントを流すため、同値通知を抑制しない
        readonly ReactiveProperty<int> beatProperty = new(0, equalityComparer: null);
        readonly ReactiveProperty<int> lengthProperty = new(DefaultLength);

        public ReadOnlyReactiveProperty<int> BeatProperty => beatProperty;
        public ReadOnlyReactiveProperty<int> LengthProperty => lengthProperty;

        public void Step()
        {
            beatProperty.Value = (beatProperty.Value + 1) % lengthProperty.Value;
        }

        public void Reset()
        {
            beatProperty.Value = 0;
        }

        public void SetLength(int length)
        {
            // beatには触らない: null comparerのbeatに代入すると偽の拍イベントが飛ぶ。
            // lengthを追い越した分は次のStepの剰余で範囲内に戻る
            lengthProperty.Value = Mathf.Clamp(length, MinLength, MaxLength);
        }

        public void Dispose()
        {
            beatProperty.Dispose();
            lengthProperty.Dispose();
        }
    }
}
