using System;
using UnityEngine;

namespace PurrNet.Packing
{
    public struct CompressedAngle : IEquatable<CompressedAngle>
    {
        public const float PRECISION = 0.0001f;

        public float value;

        public CompressedAngle(float value)
        {
            this.value = value;
        }

        public static implicit operator CompressedAngle(float value) => new CompressedAngle(value);
        public static implicit operator float(CompressedAngle angle) => angle.value;

        public CompressedAngle Round()
        {
            var copy = this;
            var rounded = Mathf.RoundToInt(value / PRECISION);
            copy.value = rounded * PRECISION;
            return copy;
        }

        public bool Equals(CompressedAngle other)
        {
            return value.Equals(other.value);
        }

        public override bool Equals(object obj)
        {
            return obj is CompressedAngle other && Equals(other);
        }

        public override int GetHashCode()
        {
            return value.GetHashCode();
        }
    }
}
