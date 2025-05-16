using System;
using PurrNet.Modules;

namespace PurrNet.Packing
{
    public static class FloatPacking
    {
        [UsedByIL]
        public static void Write(this BitPacker packer, float data)
        {
            packer.WriteBits((ulong)BitConverter.SingleToInt32Bits(data), 32);
        }

        [UsedByIL]
        public static void Read(this BitPacker packer, ref float data)
        {
            data = BitConverter.Int32BitsToSingle((int)packer.ReadBits(32));
        }

        [UsedByIL]
        private static bool WriteSingle(BitPacker packer, float oldvalue, float newvalue)
        {
            var newbits = BitConverter.SingleToInt32Bits(newvalue);
            var oldbits = BitConverter.SingleToInt32Bits(oldvalue);

            if (newbits == oldbits)
            {
                Packer<bool>.Write(packer, false);
                return false;
            }

            Packer<bool>.Write(packer, true);

            bool newSign = (newbits & 0x80000000) != 0;
            int newExp = (newbits & 0x7F800000) >> 23;
            int newMantissa = newbits & 0x007FFFFF;

            int oldExp = (oldbits & 0x7F800000) >> 23;
            int oldMantissa = oldbits & 0x007FFFFF;

            int expDiff = newExp - oldExp;
            int mantissaDiff = newMantissa - oldMantissa;

            Packer<bool>.Write(packer, newSign);
            Packer<PackedInt>.Write(packer, expDiff);
            Packer<PackedInt>.Write(packer, mantissaDiff);

            return true;
        }

        [UsedByIL]
        private static void ReadSingle(BitPacker packer, float oldvalue, ref float value)
        {
            bool hasChanged = default;
            Packer<bool>.Read(packer, ref hasChanged);

            if (!hasChanged)
            {
                value = oldvalue;
                return;
            }

            var oldbits = BitConverter.SingleToInt32Bits(oldvalue);
            int oldExp = (oldbits & 0x7F800000) >> 23;
            int oldMantissa = oldbits & 0x007FFFFF;

            bool newSign = default;
            PackedInt newExp = default;
            PackedInt newMantissa = default;

            Packer<bool>.Read(packer, ref newSign);
            Packer<PackedInt>.Read(packer, ref newExp);
            Packer<PackedInt>.Read(packer, ref newMantissa);

            newExp += oldExp;
            newMantissa += oldMantissa;

            int bits = (int)(newSign ? 0x80000000 : 0) | (newExp << 23) | newMantissa;
            value = BitConverter.Int32BitsToSingle(bits);
        }
    }
}
