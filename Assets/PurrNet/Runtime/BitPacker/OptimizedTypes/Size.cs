using System;

namespace PurrNet.Packing
{
    [Serializable]
    public struct Size : IEquatable<Size>, IPackedAuto
    {
        public ulong value;

        public Size(ulong value)
        {
            this.value = value;
        }

        public static implicit operator Size(ulong value) => new Size(value);

        public static implicit operator ulong(Size value) => value.value;

        public static implicit operator Size(long value) => new Size((ulong)value);

        public static implicit operator Size(int value) => new Size((ulong)value);

        public bool Equals(Size other)
        {
            return value == other.value;
        }

        public override bool Equals(object obj)
        {
            return obj is PackedULong other && Equals(other);
        }

        public override int GetHashCode()
        {
            return value.GetHashCode();
        }

        public override string ToString()
        {
            return $"{value}";
        }
    }
}
