using PurrNet.Modules;
using PurrNet.Pooling;

namespace PurrNet.Packing
{
    public static class MyersPackDisposableLists
    {
        [UsedByIL]
        public static bool WriteDisposableDeltaList<T>(BitPacker packer, DisposableList<T> old, DisposableList<T> value)
        {
            var scope = new DeltaWritingScope(packer);

            if (value.isDisposed)
            {
                if (value.isDisposed == old.isDisposed)
                    return scope.Complete();

                scope.Write<bool>(false);
                return scope.Complete();
            }

            DisposableList<DiffOp<T>> changes;

            if (old.isDisposed)
            {
                using var tmp = DisposableList<T>.Create();
                changes = MyersDiff.Diff(tmp, value);
            }
            else changes = MyersDiff.Diff(old, value);

            if (changes.Count > 0)
            {
                scope.Write<bool>(true);
                int count = changes.Count;
                for (int i = 0; i < count; i++)
                    scope.Write<DiffOp<T>>(changes[i]);
                scope.Write(DiffOp<T>.FinalOperation());
            }

            return scope.Complete();
        }

        [UsedByIL]
        public static void ReadDisposableDeltaList<T>(BitPacker packer, DisposableList<T> old, ref DisposableList<T> value)
        {
            if (!DeltaReadingScope.Continue(packer, old, ref value))
                return;

            bool hasValue = Packer<bool>.Read(packer);

            if (!hasValue)
            {
                value.Dispose();
                return;
            }

            if (!old.isDisposed)
            {
                if (value.isDisposed)
                {
                    value = DisposableList<T>.Create(old.Count);
                    value.AddRange(old);
                }
                else if (value.list != old.list)
                {
                    value.Clear();
                    value.AddRange(old);
                }
            }
            else if (value.isDisposed)
            {
                value = DisposableList<T>.Create();
            }
            else value.Clear();

            var changes = DisposableList<DiffOp<T>>.Create();

            while (packer.positionInBits < packer.length * 8)
            {
                DiffOp<T> operation = default;
                packer.ReadOperation(ref operation);
                if (operation.type == OperationType.End)
                    break;
                changes.Add(operation);
            }

            MyersDiff.Apply(value, changes);

            changes.Dispose();
        }
    }
}
