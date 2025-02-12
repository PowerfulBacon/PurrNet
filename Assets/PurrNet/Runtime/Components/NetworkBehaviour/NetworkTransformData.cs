using System;
using PurrNet.Logging;
using PurrNet.Packing;
using UnityEngine;

namespace PurrNet
{
    public struct NetworkTransformData : IEquatable<NetworkTransformData>
    {
        public Vector3 position;
        public HalfQuaternion rotation;
        public HalfVector3 scale;
        
        public NetworkTransformData(Vector3 position, Quaternion rotation, Vector3 scale)
        {
            this.position = position;
            this.rotation = rotation;
            this.scale = scale;
        }

        public override bool Equals(object obj)
        {
            return obj is NetworkTransformData other && Equals(other);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(position, rotation, scale);
        }

        public bool Equals(NetworkTransformData other)
        {
            return position.Equals(other.position) && rotation.Equals(other.rotation) && scale.Equals(other.scale);
        }

        public override string ToString()
        {
            return $"Position: {position}, Rotation: {rotation}, Scale: {scale}";
        }

        public bool IsDifferent(NetworkTransformData currentData)
        {
            if (Mathf.Abs(position.x - currentData.position.x) > DeltaPackFloats.PRECISION ||
                Mathf.Abs(position.y - currentData.position.y) > DeltaPackFloats.PRECISION ||
                Mathf.Abs(position.z - currentData.position.z) > DeltaPackFloats.PRECISION)
            {
                return true;
            }

            if (Quaternion.Angle(rotation, currentData.rotation) > DeltaPackFloats.PRECISION)
            {
                return true;
            }
            
            if (Mathf.Abs(scale.x - currentData.scale.x) > DeltaPackFloats.PRECISION ||
                Mathf.Abs(scale.y - currentData.scale.y) > DeltaPackFloats.PRECISION ||
                Mathf.Abs(scale.z - currentData.scale.z) > DeltaPackFloats.PRECISION)
            {
                return true;
            }
            
            return false;
        }
    }
}