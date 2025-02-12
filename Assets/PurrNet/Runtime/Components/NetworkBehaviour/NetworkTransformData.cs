using System;
using PurrNet.Packing;
using UnityEngine;

namespace PurrNet
{
    public struct NetworkTransformData : IEquatable<NetworkTransformData>
    {
        public Vector3 position;
        public HalfQuaternion rotation;
        public HalfVector3 scale;
        
        public NetworkTransformData(Vector3 position, HalfQuaternion rotation, HalfVector3 scale)
        {
            this.position = position;
            this.rotation = rotation;
            this.scale = scale;
        }

        public override bool Equals(object obj)
        {
            return obj is NetworkTransformData other && Equals(other);
        }
        
        public bool IsSimilar(NetworkTransformData other)
        {
            bool isPositionSimilar = Vector3.SqrMagnitude(position - other.position) < 0.001f;
            bool isRotationSimilar = Quaternion.Dot(rotation, other.rotation) > 0.999f;
            bool isScaleSimilar = Vector3.SqrMagnitude(scale - other.scale) < 0.01f;
            return isPositionSimilar && isRotationSimilar && isScaleSimilar;
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
    }
}