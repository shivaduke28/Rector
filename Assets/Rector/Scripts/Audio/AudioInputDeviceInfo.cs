using System;

namespace Rector.Audio
{
    public readonly struct AudioInputDeviceInfo : IEquatable<AudioInputDeviceInfo>
    {
        public readonly string Name;
        public readonly string Id;

        public AudioInputDeviceInfo(string name, string id)
        {
            Name = name;
            Id = id;
        }

        public bool Equals(AudioInputDeviceInfo other)
        {
            return Id == other.Id;
        }

        public override bool Equals(object obj)
        {
            return obj is AudioInputDeviceInfo other && Equals(other);
        }

        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }

        public bool IsValid => !string.IsNullOrEmpty(Id);
        public static AudioInputDeviceInfo Empty => new("", "");
    }
}
