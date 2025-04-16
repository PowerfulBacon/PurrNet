using PurrNet.Modules;
using UnityEngine;

namespace PurrNet.Packing
{
    public static class PackingIntegers
    {
        public static byte ZigzagEncode(sbyte i) => (byte)(((uint)i >> 7) ^ ((uint)i << 1));

        public static sbyte ZigzagDecode(byte i) => (sbyte)((i >> 1) ^ -(i & 1));

        public static ushort ZigzagEncode(short i) => (ushort)(((ulong)i >> 15) ^ ((ulong)i << 1));

        public static short ZigzagDecode(ushort i) => (short)((i >> 1) ^ -(i & 1));

        public static uint ZigzagEncode(int i) => (uint)(((ulong)i >> 31) ^ ((ulong)i << 1));

        public static ulong ZigzagEncode(long i) => (ulong)((i >> 63) ^ (i << 1));

        public static int ZigzagDecode(uint i) => (int)(((long)i >> 1) ^ -((long)i & 1));

        public static long ZigzagDecode(ulong i) => ((long)(i >> 1) & 0x7FFFFFFFFFFFFFFFL) ^ ((long)(i << 63) >> 63);

        static int CountLeadingZeroBits(ulong value)
        {
            if (value == 0) return 64; // Special case for zero

            int count = 0;
            if ((value & 0xFFFFFFFF00000000) == 0)
            {
                count += 32;
                value <<= 32;
            }

            if ((value & 0xFFFF000000000000) == 0)
            {
                count += 16;
                value <<= 16;
            }

            if ((value & 0xFF00000000000000) == 0)
            {
                count += 8;
                value <<= 8;
            }

            if ((value & 0xF000000000000000) == 0)
            {
                count += 4;
                value <<= 4;
            }

            if ((value & 0xC000000000000000) == 0)
            {
                count += 2;
                value <<= 2;
            }

            if ((value & 0x8000000000000000) == 0)
            {
                count += 1;
            }

            return count;
        }

        [UsedByIL]
        public static void Write(BitPacker packer, PackedUInt value)
        {
            Write(packer, new PackedULong(value.value));
        }

        [UsedByIL]
        public static void Read(BitPacker packer, ref PackedUInt value)
        {
            PackedULong packed = default;
            Read(packer, ref packed);
            value = new PackedUInt((uint)packed.value);
        }

        [UsedByIL]
        public static void Write(BitPacker packer, PackedInt value)
        {
            var packed = new PackedUInt(ZigzagEncode(value.value));
            Write(packer, packed);
        }

        [UsedByIL]
        public static void Read(BitPacker packer, ref PackedInt value)
        {
            PackedUInt packed = default;
            Read(packer, ref packed);
            value = new PackedInt(ZigzagDecode(packed.value));
        }

        [UsedByIL]
        public static void Write(BitPacker packer, PackedUShort value)
        {
            var packed = new PackedULong(value.value);
            Write(packer, packed);
        }

        [UsedByIL]
        public static void Read(BitPacker packer, ref PackedUShort value)
        {
            PackedULong packed = default;
            Read(packer, ref packed);
            value = new PackedUShort((ushort)packed.value);
        }

        [UsedByIL]
        public static void Write(BitPacker packer, PackedShort value)
        {
            Write(packer, new PackedULong(ZigzagEncode(value.value)));
        }

        [UsedByIL]
        public static void Read(BitPacker packer, ref PackedShort value)
        {
            PackedULong packed = default;
            Read(packer, ref packed);
            value = new PackedShort((short)ZigzagDecode(packed.value));
        }

        private const int SEGMENTS = 16;
        const int TOTAL_BITS = 64;
        const int CHUNK = TOTAL_BITS / SEGMENTS;

        public static void WriteSmallSegmented(BitPacker packer, long value, byte segmentedBits)
        {
            ulong packedValue = ZigzagEncode(value);
            WriteSmallSegmented(packer, packedValue, segmentedBits);
        }

        public static void WriteSmallSegmented(BitPacker packer, ulong value, byte segmentedBits)
        {
            if (value == 0) {
                packer.WriteBits(0, 1); // Just 1 bit for zero
                return;
            }
            if (value == 1) {
                packer.WriteBits(1, 1); // 1 bit for one
                packer.WriteBits(0, 1);
                return;
            }
            packer.WriteBits(1, 1); // Signal non-zero/one
            packer.WriteBits(1, 1); // Signal larger value
            WriteSegmented(packer, value - 2, segmentedBits); // Offset and use segments
        }

        public static void ReadSmallSegmented(BitPacker packer, ref long value, byte segmentedBits)
        {
            ulong packedValue = 0;
            ReadSmallSegmented(packer, ref packedValue, segmentedBits);
            value = ZigzagDecode(packedValue);
        }

        public static void ReadSmallSegmented(BitPacker packer, ref ulong value, byte segmentedBits)
        {
            // Read first bit
            if (packer.ReadBits(1) == 0)
            {
                // Value is 0
                value = 0;
                return;
            }

            // Read second bit
            if (packer.ReadBits(1) == 0)
            {
                // Value is 1
                value = 1;
                return;
            }

            // Value is larger than 1
            ulong result = 0;
            ReadSegmented(packer, ref result, segmentedBits);
            value = result + 2; // Add back the offset
        }

        public static void WriteSegmented(BitPacker packer, long value, byte segments)
        {
            ulong packedValue = ZigzagEncode(value);
            WriteSegmented(packer, packedValue, segments);
        }

        static readonly ulong[] _masks = {
            0x00000000UL, 0x00000001UL, 0x00000003UL, 0x00000007UL,
            0x0000000fUL, 0x0000001fUL, 0x0000003fUL, 0x0000007fUL,
            0x000000ffUL, 0x000001ffUL, 0x000003ffUL, 0x000007ffUL,
            0x00000fffUL, 0x00001fffUL, 0x00003fffUL, 0x00007fffUL,
            0x0000ffffUL, 0x0001ffffUL, 0x0003ffffUL, 0x0007ffffUL,
            0x000fffffUL, 0x001fffffUL, 0x003fffffUL, 0x007fffffUL,
            0x00ffffffUL, 0x01ffffffUL, 0x03ffffffUL, 0x07ffffffUL,
            0x0fffffffUL, 0x1fffffffUL, 0x3fffffffUL, 0x7fffffffUL,
            0xffffffffUL
        };

        public static void WritePrefixed(BitPacker packer, long value, byte maxBitCount)
        {
            ulong packedValue = ZigzagEncode(value);
            WritePrefixed(packer, packedValue, maxBitCount);
        }

        public static void ReadPrefixed(BitPacker packer, ref long value, byte maxBitCount)
        {
            ulong packedValue = 0;
            ReadPrefixed(packer, ref packedValue, maxBitCount);
            value = ZigzagDecode(packedValue);
        }

        public static int SimulatePrefixed(long value, byte maxBitCount)
        {
            ulong packedValue = ZigzagEncode(value);
            return SimulatePrefixed(packedValue, maxBitCount);
        }

        public static int SimulatePrefixed(ulong value, byte maxBitCount)
        {
            byte bitCount = (byte)(TOTAL_BITS - CountLeadingZeroBits(value));
            byte prefixBits = (byte)Mathf.CeilToInt(Mathf.Log(maxBitCount, 2));
            if (bitCount == 0)
                return prefixBits;
            return bitCount + prefixBits;
        }

        public static void WritePrefixed(BitPacker packer, ulong value, byte maxBitCount)
        {
            byte bitCount = (byte)(TOTAL_BITS - CountLeadingZeroBits(value));
            byte prefixBits = (byte)Mathf.CeilToInt(Mathf.Log(maxBitCount, 2));

            packer.WriteBits(bitCount, prefixBits);

            if (bitCount == 0)
                return;

            packer.WriteBits(value, bitCount);
        }

        public static void ReadPrefixed(BitPacker packer, ref ulong value, byte maxBitCount)
        {
            byte prefixBits = (byte)Mathf.CeilToInt(Mathf.Log(maxBitCount, 2));

            if (prefixBits == 0)
            {
                value = 0;
                return;
            }

            byte bitCount = (byte)packer.ReadBits(prefixBits);
            value = packer.ReadBits(bitCount);
        }

        public static int SimulateSegmented(long value, byte segments)
        {
            ulong packedValue = ZigzagEncode(value);
            return SimulateSegmented(packedValue, segments);
        }

        public static int SimulateSegmented(ulong value, byte segments)
        {
            int writtenBits = 0;
            do
            {
                ulong next = value >> segments;
                bool isLastSegment = next == 0;
                writtenBits += segments + 1;
                value = next;

                if (isLastSegment)
                    break;

            }
            while (true);
            return writtenBits;
        }

        public static void WriteSegmented(BitPacker packer, ulong value, byte segments)
        {
            ulong mask = _masks[segments];

            do
            {
                ulong next = value >> segments;
                bool isLastSegment = next == 0;
                packer.WriteBits((byte)(isLastSegment ? 0 : 1), 1);
                packer.WriteBits(value & mask, segments);
                value = next;

                if (isLastSegment)
                    break;
            }
            while (true);
        }

        public static void ReadSegmented(BitPacker packer, ref long value, byte segments)
        {
            ulong packedValue = 0;
            ReadSegmented(packer, ref packedValue, segments);
            value = ZigzagDecode(packedValue);
        }

        public static void ReadSegmented(BitPacker packer, ref ulong value, byte segments)
        {
            ulong result = 0;
            ulong multiplier = 1;
            int maxIterations = Mathf.CeilToInt(TOTAL_BITS / (float)segments);
            int iteration = 0;

            do
            {
                bool continueReading = packer.ReadBits(1) == 1;
                ulong chunk = packer.ReadBits(segments);
                result |= chunk * multiplier;
                multiplier <<= segments;

                if (!continueReading)
                    break;

                iteration++;
            } while (iteration < maxIterations);

            value = result;
        }


        [UsedByIL]
        public static void Write(BitPacker packer, PackedULong value)
        {
            int trailingZeroes = CountLeadingZeroBits(value.value);
            int emptyChunks = trailingZeroes / CHUNK;
            int segmentCount = SEGMENTS - emptyChunks;
            int pointer = 0;

            if (segmentCount == 0)
            {
                packer.WriteBits(0, 1);
                return;
            }

            packer.WriteBits(1, 1);

            const ulong mask = (ulong.MaxValue >> (TOTAL_BITS - CHUNK));
            while (segmentCount > 0 && pointer < TOTAL_BITS)
            {
                ulong isolated = (value.value >> pointer) & mask;
                packer.WriteBits(isolated, CHUNK);
                pointer += CHUNK;

                --segmentCount;
                packer.WriteBits(segmentCount == 0 ? 0u : 1u, 1);
            }
        }

        [UsedByIL]
        public static void Read(BitPacker packer, ref PackedULong value)
        {
            if (packer.ReadBits(1) == 0)
            {
                value.value = 0;
                return;
            }

            ulong result = 0;
            int pointer = 0;
            bool continueReading;

            do
            {
                ulong chunk = packer.ReadBits(CHUNK);
                result |= chunk << pointer;
                pointer += CHUNK;

                continueReading = packer.ReadBits(1) == 1;
            } while (continueReading && pointer < TOTAL_BITS);

            value.value = result;
        }

        [UsedByIL]
        public static void Write(BitPacker packer, PackedLong value)
        {
            Write(packer, new PackedULong(ZigzagEncode(value.value)));
        }

        [UsedByIL]
        public static void Read(BitPacker packer, ref PackedLong value)
        {
            PackedULong packed = default;
            Read(packer, ref packed);
            value = new PackedLong(ZigzagDecode(packed.value));
        }

        [UsedByIL]
        public static void Write(BitPacker packer, PackedByte value)
        {
            Write(packer, new PackedULong(value.value));
        }

        [UsedByIL]
        public static void Read(BitPacker packer, ref PackedByte value)
        {
            PackedULong packed = default;
            Read(packer, ref packed);
            value = new PackedByte((byte)packed.value);
        }

        [UsedByIL]
        public static void Write(BitPacker packer, PackedSByte value)
        {
            Write(packer, new PackedByte(ZigzagEncode(value.value)));
        }

        [UsedByIL]
        public static void Read(BitPacker packer, ref PackedSByte value)
        {
            PackedByte packed = default;
            Read(packer, ref packed);
            value = new PackedSByte(ZigzagDecode(packed.value));
        }
    }
}
