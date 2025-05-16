using System;
using System.Collections;
using System.Collections.Generic;

namespace PurrNet.Pooling
{
    public struct DisposableList<T> : IList<T>, IDisposable
    {
        private readonly bool _shouldDispose;

        public List<T> list { get; }

        public DisposableList(int capacity)
        {
            var newList = ListPool<T>.Instantiate();

            if (newList.Capacity < capacity)
                newList.Capacity = capacity;

            list = newList;
            _isAllocated = true;
            _shouldDispose = true;
        }

        public void AddRange(IEnumerable<T> collection)
        {
            if (isDisposed) throw new ObjectDisposedException(nameof(DisposableList<T>));
            foreach (var item in collection)
                list.Add(item);
        }

        public void Dispose()
        {
            if (isDisposed) return;

            if (_shouldDispose && list != null)
                ListPool<T>.Destroy(list);
            _isAllocated = false;
        }

        public IEnumerator<T> GetEnumerator()
        {
            if (isDisposed) throw new ObjectDisposedException(nameof(DisposableList<T>));
            return list.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            if (isDisposed) throw new ObjectDisposedException(nameof(DisposableList<T>));
            return GetEnumerator();
        }

        public void Add(T item)
        {
            if (isDisposed) throw new ObjectDisposedException(nameof(DisposableList<T>));
            list.Add(item);
        }

        public void Clear()
        {
            if (isDisposed) throw new ObjectDisposedException(nameof(DisposableList<T>));
            list.Clear();
        }

        public bool Contains(T item)
        {
            if (isDisposed) throw new ObjectDisposedException(nameof(DisposableList<T>));
            return list.Contains(item);
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            if (isDisposed) throw new ObjectDisposedException(nameof(DisposableList<T>));
            list.CopyTo(array, arrayIndex);
        }

        public bool Remove(T item)
        {
            if (isDisposed) throw new ObjectDisposedException(nameof(DisposableList<T>));
            return list.Remove(item);
        }

        public int Count
        {
            get
            {
                if (isDisposed) throw new ObjectDisposedException(nameof(DisposableList<T>));
                return list.Count;
            }
        }

        public bool IsReadOnly
        {
            get
            {
                if (isDisposed) throw new ObjectDisposedException(nameof(DisposableList<T>));
                return false;
            }
        }

        private bool _isAllocated;

        public bool isDisposed => !_isAllocated;

        public int IndexOf(T item)
        {
            if (isDisposed) throw new ObjectDisposedException(nameof(DisposableList<T>));
            return list.IndexOf(item);
        }

        public void Insert(int index, T item)
        {
            if (isDisposed) throw new ObjectDisposedException(nameof(DisposableList<T>));
            list.Insert(index, item);
        }

        public void RemoveAt(int index)
        {
            if (isDisposed) throw new ObjectDisposedException(nameof(DisposableList<T>));
            list.RemoveAt(index);
        }

        public T this[int index]
        {
            get
            {
                if (isDisposed) throw new ObjectDisposedException(nameof(DisposableList<T>));
                return list[index];
            }
            set
            {
                if (isDisposed) throw new ObjectDisposedException(nameof(DisposableList<T>));
                list[index] = value;
            }
        }
    }
}
