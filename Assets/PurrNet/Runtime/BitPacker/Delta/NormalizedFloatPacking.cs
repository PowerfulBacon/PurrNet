using PurrNet.Modules;

namespace PurrNet.Packing
{
    public static class NormalizedFloatPacking
    {

        [UsedByIL]
        private static void WriteHalf(BitPacker packer, NormalizedFloat value)
        {
            PackingIntegers.WritePrefixed(packer, value.value, NormalizedFloat.BIT_RESOLUTION);
        }

        [UsedByIL]
        private static void ReadHalf(BitPacker packer, ref NormalizedFloat value)
        {
            PackingIntegers.ReadPrefixed(packer, ref value.value, NormalizedFloat.BIT_RESOLUTION);
        }

        const int MAX_SEGMENTED = 3;

        [UsedByIL]
        private static bool WriteAngle(BitPacker packer, NormalizedFloat oldvalue, NormalizedFloat newvalue)
        {
            var delta = newvalue.value - oldvalue.value;

            if (delta == 0)
            {
                packer.WriteBits(0, 1);
                return false;
            }

            packer.WriteBits(1, 1);
            PackingIntegers.WritePrefixed(packer, delta, NormalizedFloat.BIT_RESOLUTION);
            return true;
        }

        [UsedByIL]
        private static void ReadAngle(BitPacker packer, NormalizedFloat oldvalue, ref NormalizedFloat value)
        {
            if (packer.ReadBits(1) == 0)
                return;

            long delta = default;
            PackingIntegers.ReadPrefixed(packer, ref delta, NormalizedFloat.BIT_RESOLUTION);
            value.value = delta + oldvalue.value;
        }
    }
}
