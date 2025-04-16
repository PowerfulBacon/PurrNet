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
            long newExp = (newbits & 0x7F800000) >> 23;
            long newMantissa = newbits & 0x007FFFFF;

            long oldExp = (oldbits & 0x7F800000) >> 23;
            long oldMantissa = oldbits & 0x007FFFFF;

            long expDiff = newExp - oldExp;
            long mantissaDiff = newMantissa - oldMantissa;

            Packer<bool>.Write(packer, newSign);
            PackingIntegers.WriteSegmented(packer, expDiff, 2, 8);
            PackingIntegers.WriteSegmented(packer, mantissaDiff, 9, 23);

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
            long oldExp = (oldbits & 0x7F800000) >> 23;
            long oldMantissa = oldbits & 0x007FFFFF;

            bool newSign = default;
            long newExp = default;
            long newMantissa = default;

            Packer<bool>.Read(packer, ref newSign);
            PackingIntegers.ReadSegmented(packer, ref newExp, 2, 8);
            PackingIntegers.ReadSegmented(packer, ref newMantissa, 9, 23);

            newExp += oldExp;
            newMantissa += oldMantissa;

            int bits = (int)(newSign ? 0x80000000 : 0) | ((int)newExp << 23) | (int)newMantissa;
            value = BitConverter.Int32BitsToSingle(bits);
        }
    }
}
