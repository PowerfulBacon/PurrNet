using PurrNet.Modules;
using PurrNet.Pooling;

namespace PurrNet.Packing
{
    public static class PackDisposableLists
    {
        [UsedByIL]
        public static void WriteDisposableList<T>(this BitPacker packer, DisposableList<T> value)
        {
            if (value.isDisposed || value.list == null)
            {
                Packer<bool>.Write(packer, false);
                return;
            }

            Packer<bool>.Write(packer, true);

            uint length = (uint)value.Count;
            Packer<PackedUInt>.Write(packer, length);

            for (int i = 0; i < length; i++)
                Packer<T>.Write(packer, value[i]);
        }

        [UsedByIL]
        public static void ReadDisposableList<T>(this BitPacker packer, ref DisposableList<T> value)
        {
            value.Dispose();

            bool hasValue = default;

            packer.Read(ref hasValue);

            if (!hasValue)
                return;

            PackedUInt length = default;
            Packer<PackedUInt>.Read(packer, ref length);
            value = new DisposableList<T>((int)length.value);

            for (int i = 0; i < length; i++)
            {
                T item = default;
                Packer<T>.Read(packer, ref item);
                value.Add(item);
            }
        }

        [UsedByIL]
        public static bool WriteDisposableDeltaList<T>(this BitPacker packer, DisposableList<T> old, DisposableList<T> value)
        {
            if (Packer.AreEqual(old, value))
            {
                Packer<bool>.Write(packer, true);
                return false;
            }

            Packer<bool>.Write(packer, false);
            WriteDisposableList(packer, value);
            return true;
        }

        [UsedByIL]
        public static void ReadDisposableDeltaList<T>(this BitPacker packer, DisposableList<T> old, ref DisposableList<T> value)
        {
            bool areEqual = default;
            packer.Read(ref areEqual);

            if (areEqual)
            {
                value.Dispose();
                value = Packer.Copy(old);
                return;
            }

            /*uint isEqualUpTo = 0;

            for (int i = 0; i < old.Count; i++, isEqualUpTo++)
            {
                if (!Packer.AreEqual(old[i], value[i]))
                    break;
            }

            Packer<PackedUInt>.Write(packer, isEqualUpTo);*/

            ReadDisposableList(packer, ref value);
        }
    }
}
