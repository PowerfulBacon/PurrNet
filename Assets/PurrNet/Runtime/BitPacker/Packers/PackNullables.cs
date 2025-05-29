namespace PurrNet.Packing
{
    public static class PackNullables
    {
        public static bool WriteDeltaNullable<T>(BitPacker packer, T? oldvalue, T? newvalue) where T : struct
        {
            int flagPos = packer.AdvanceBits(1);

            bool hasChanged = DeltaPacker<bool>.Write(packer, oldvalue.HasValue, newvalue.HasValue);
            hasChanged = DeltaPacker<T>.Write(packer, oldvalue.GetValueOrDefault(), newvalue.GetValueOrDefault()) || hasChanged;

            packer.WriteAt(flagPos, hasChanged);

            if (!hasChanged)
                packer.SetBitPosition(flagPos + 1);

            return hasChanged;
        }

        public static void ReadDeltaNullable<T>(BitPacker packer, T? oldvalue, ref T? value) where T : struct
        {
            bool hasChanged = default;
            Packer<bool>.Read(packer, ref hasChanged);

            if (hasChanged)
            {
                bool isNull = default;
                T readValue = default;

                DeltaPacker<bool>.Read(packer, oldvalue.HasValue, ref isNull);
                DeltaPacker<T>.Read(packer, oldvalue.GetValueOrDefault(), ref readValue);

                value = isNull ? null : readValue;
            }
            else
            {
                value = oldvalue;
            }
        }

        public static void WriteNullable<T>(BitPacker packer, T? value) where T : struct
        {
            if (!value.HasValue)
            {
                Packer<bool>.Write(packer, false);
                return;
            }

            Packer<bool>.Write(packer, true);
            Packer<T>.Write(packer, value.Value);
        }

        public static void ReadNullable<T>(BitPacker packer, ref T? value) where T : struct
        {
            bool hasValue = default;
            packer.Read(ref hasValue);

            if (!hasValue)
            {
                value = null;
                return;
            }

            T val = default;
            Packer<T>.Read(packer, ref val);
            value = val;
        }
    }
}
