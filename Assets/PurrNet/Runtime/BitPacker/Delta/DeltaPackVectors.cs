using PurrNet.Modules;
using UnityEngine;

namespace PurrNet.Packing
{
    public static class DeltaPackVectors
    {
        [UsedByIL]
        private static void WriteVector2(BitPacker packer, Vector2 oldvalue, Vector2 newvalue)
        {
            bool hasChanged = oldvalue != newvalue;
            Packer<bool>.Write(packer, hasChanged);

            if (hasChanged)
            {
                DeltaPacker<float>.Write(packer, oldvalue.x, newvalue.x);
                DeltaPacker<float>.Write(packer, oldvalue.y, newvalue.y);
            }
        }
        
        [UsedByIL]
        private static void ReadVector2(BitPacker packer, Vector2 oldvalue, ref Vector2 value)
        {
            bool hasChanged = default;
            Packer<bool>.Read(packer, ref hasChanged);

            if (hasChanged)
            {
                DeltaPacker<float>.Read(packer, oldvalue.x, ref value.x);
                DeltaPacker<float>.Read(packer, oldvalue.y, ref value.y);
            }
            else value = oldvalue;
        }
        
        [UsedByIL]
        private static void WriteVector3(BitPacker packer, Vector3 oldvalue, Vector3 newvalue)
        {
            bool hasChanged = oldvalue != newvalue;
            Packer<bool>.Write(packer, hasChanged);

            if (hasChanged)
            {
                DeltaPacker<float>.Write(packer, oldvalue.x, newvalue.x);
                DeltaPacker<float>.Write(packer, oldvalue.y, newvalue.y);
                DeltaPacker<float>.Write(packer, oldvalue.z, newvalue.z);
            }
        }
        
        [UsedByIL]
        private static void ReadVector3(BitPacker packer, Vector3 oldvalue, ref Vector3 value)
        {
            bool hasChanged = default;
            Packer<bool>.Read(packer, ref hasChanged);

            if (hasChanged)
            {
                DeltaPacker<float>.Read(packer, oldvalue.x, ref value.x);
                DeltaPacker<float>.Read(packer, oldvalue.y, ref value.y);
                DeltaPacker<float>.Read(packer, oldvalue.z, ref value.z);
            }
            else value = oldvalue;
        }
        
        [UsedByIL]
        private static void WriteVector4(BitPacker packer, Vector4 oldvalue, Vector4 newvalue)
        {
            bool hasChanged = oldvalue != newvalue;
            Packer<bool>.Write(packer, hasChanged);

            if (hasChanged)
            {
                DeltaPacker<float>.Write(packer, oldvalue.x, newvalue.x);
                DeltaPacker<float>.Write(packer, oldvalue.y, newvalue.y);
                DeltaPacker<float>.Write(packer, oldvalue.z, newvalue.z);
                DeltaPacker<float>.Write(packer, oldvalue.w, newvalue.w);
            }
        }
        
        [UsedByIL]
        private static void ReadVector4(BitPacker packer, Vector4 oldvalue, ref Vector4 value)
        {
            bool hasChanged = default;
            Packer<bool>.Read(packer, ref hasChanged);

            if (hasChanged)
            {
                DeltaPacker<float>.Read(packer, oldvalue.x, ref value.x);
                DeltaPacker<float>.Read(packer, oldvalue.y, ref value.y);
                DeltaPacker<float>.Read(packer, oldvalue.z, ref value.z);
                DeltaPacker<float>.Read(packer, oldvalue.w, ref value.w);
            }
            else value = oldvalue;
        }
        
        [UsedByIL]
        private static void WriteQuaternion(BitPacker packer, Quaternion oldvalue, Quaternion newvalue)
        {
            newvalue.Normalize();
            
            bool hasChanged = oldvalue != newvalue;
            Packer<bool>.Write(packer, hasChanged);

            if (hasChanged)
            {
                DeltaPacker<float>.Write(packer, oldvalue.x, newvalue.x);
                DeltaPacker<float>.Write(packer, oldvalue.y, newvalue.y);
                DeltaPacker<float>.Write(packer, oldvalue.z, newvalue.z);
                DeltaPacker<bool>.Write(packer, oldvalue.w < 0, newvalue.w < 0);
            }
        }
        
        [UsedByIL]
        private static void ReadQuaternion(BitPacker packer, Quaternion oldvalue, ref Quaternion value)
        {
            bool hasChanged = default;
            Packer<bool>.Read(packer, ref hasChanged);

            if (hasChanged)
            {
                DeltaPacker<float>.Read(packer, oldvalue.x, ref value.x);
                DeltaPacker<float>.Read(packer, oldvalue.y, ref value.y);
                DeltaPacker<float>.Read(packer, oldvalue.z, ref value.z);
                
                bool oldW = oldvalue.w < 0;
                DeltaPacker<bool>.Read(packer, oldW, ref oldW);
                
                var w = (Half)Mathf.Sqrt(Mathf.Max(0, 1 - value.x * value.x - value.y * value.y - value.z * value.z));
            
                if (oldW)
                     value.w = -w;
                else value.w = w;
            }
            else value = oldvalue;
        }
        
        [UsedByIL]
        private static void WriteQuaternion(BitPacker packer, HalfQuaternion oldvalue, HalfQuaternion newvalue)
        {
            bool hasChanged = !oldvalue.Equals(newvalue);
            Packer<bool>.Write(packer, hasChanged);

            if (hasChanged)
            {
                DeltaPacker<Half>.Write(packer, oldvalue.x, newvalue.x);
                DeltaPacker<Half>.Write(packer, oldvalue.y, newvalue.y);
                DeltaPacker<Half>.Write(packer, oldvalue.z, newvalue.z);
                DeltaPacker<Half>.Write(packer, oldvalue.w, newvalue.w);
            }
        }
        
        [UsedByIL]
        private static void ReadQuaternion(BitPacker packer, HalfQuaternion oldvalue, ref HalfQuaternion value)
        {
            bool hasChanged = default;
            Packer<bool>.Read(packer, ref hasChanged);

            if (hasChanged)
            {
                DeltaPacker<Half>.Read(packer, oldvalue.x, ref value.x);
                DeltaPacker<Half>.Read(packer, oldvalue.y, ref value.y);
                DeltaPacker<Half>.Read(packer, oldvalue.z, ref value.z);
                DeltaPacker<Half>.Read(packer, oldvalue.w, ref value.w);
            }
            else value = oldvalue;
        }
    }
}
