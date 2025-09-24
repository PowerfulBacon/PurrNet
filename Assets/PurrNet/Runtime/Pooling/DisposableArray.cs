using System;
using System.Buffers;
using System.Collections;
using System.Collections.Generic;

namespace PurrNet.Pooling
{
    public struct DisposableArray<T> : IDisposable, IReadOnlyList<T>
    {
        private bool _shouldDispose;

        public T[] array { get; private set; }

        public int Count { get; private set; }

        public bool isDisposed => !_shouldDispose;

        public T this[int index]
        {
            get
            {
                if (isDisposed) throw new ObjectDisposedException(nameof(DisposableArray<T>));
                if (index >= Count || index < 0)
                    throw new IndexOutOfRangeException($"Index {index} is out of range for list of size {Count}.");
                return array[index];
            }
            set
            {
                if (isDisposed) throw new ObjectDisposedException(nameof(DisposableArray<T>));
                if (index >= Count || index < 0)
                    throw new IndexOutOfRangeException($"Index {index} is out of range for list of size {Count}.");
                array[index] = value;
            }
        }

        public static DisposableArray<T> Create(int size) =>
            new()
            {
                array = ArrayPool<T>.Shared.Rent(size),
                Count = size,
                _shouldDispose = true
            };

        public static DisposableArray<T> Create(DisposableArray<T> copyFrom)
        {
            var array = ArrayPool<T>.Shared.Rent(copyFrom.Count);
            Array.Copy(copyFrom.array, array, copyFrom.Count);
            return new DisposableArray<T>
            {
                array = array,
                Count = copyFrom.Count,
                _shouldDispose = true
            };
        }

        public static DisposableArray<T> Create(T[] copyFrom)
        {
            var array = ArrayPool<T>.Shared.Rent(copyFrom.Length);
            Array.Copy(copyFrom, array, copyFrom.Length);
            return new DisposableArray<T>
            {
                array = array,
                Count = copyFrom.Length,
                _shouldDispose = true
            };
        }

        public void Dispose()
        {
            if (!_shouldDispose) return;
            ArrayPool<T>.Shared.Return(array);
            _shouldDispose = false;
        }

        public IEnumerator<T> GetEnumerator()
        {
            if (isDisposed) throw new ObjectDisposedException(nameof(DisposableArray<T>));
            for (int i = 0; i < Count; i++)
                yield return array[i];
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
