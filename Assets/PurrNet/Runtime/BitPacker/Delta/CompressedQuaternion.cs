using System;
using UnityEngine;

namespace PurrNet.Packing
{
    public struct CompressedQuaternion : IEquatable<CompressedQuaternion>
    {
        public NormalizedFloat x;
        public NormalizedFloat y;
        public NormalizedFloat z;
        public NormalizedFloat w;

        public CompressedQuaternion(NormalizedFloat x, NormalizedFloat y, NormalizedFloat z, NormalizedFloat w)
        {
            this.x = x;
            this.y = y;
            this.z = z;
            this.w = w;
        }

        public static implicit operator CompressedQuaternion(Quaternion value)
        {
            value.Normalize();
            return new CompressedQuaternion(value.x, value.y, value.z, value.w);
        }

        public static implicit operator Quaternion(CompressedQuaternion angle) => new Quaternion(angle.x.GetValue(), angle.y.GetValue(), angle.z.GetValue(), angle.w.GetValue());

        public bool Equals(CompressedQuaternion other)
        {
            return x.Equals(other.x) && y.Equals(other.y) && z.Equals(other.z) && w.Equals(other.w);
        }

        public override bool Equals(object obj)
        {
            return obj is CompressedQuaternion other && Equals(other);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(x, y, z, w);
        }
    }
}
