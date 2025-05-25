namespace PurrNet.Packing
{
    public static class PackNullables
    {
        public static bool WriteDeltaNullable<T>(BitPacker packer, T? oldvalue, T? newvalue) where T : struct
        {
            bool hasChanged = oldvalue.HasValue != newvalue.HasValue;
            Packer<bool>.Write(packer, hasChanged);

            if (hasChanged)
                WriteNullable(packer, newvalue);

            return hasChanged;
        }

        public static void ReadDeltaNullable<T>(BitPacker packer, T? oldvalue, ref T? value) where T : struct
        {
            bool hasChanged = default;
            packer.Read(ref hasChanged);

            if (hasChanged)
                ReadNullable(packer, ref value);
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
