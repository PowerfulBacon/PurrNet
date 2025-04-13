using System;
using UnityEngine;

namespace PurrNet.Packing
{
    public struct CompressedQuaternion : IEquatable<CompressedQuaternion>
    {
        public CompressedAngle x;
        public CompressedAngle y;
        public CompressedAngle z;
        public CompressedAngle w;

        public CompressedQuaternion(CompressedAngle x, CompressedAngle y, CompressedAngle z, CompressedAngle w)
        {
            this.x = x.Round();
            this.y = y.Round();
            this.z = z.Round();
            this.w = w.Round();
        }

        public static implicit operator CompressedQuaternion(Quaternion value) => new CompressedQuaternion(value.x, value.y, value.z, value.w);
        public static implicit operator Quaternion(CompressedQuaternion angle) => new Quaternion(angle.x.value, angle.y.value, angle.z.value, angle.w.value);

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
