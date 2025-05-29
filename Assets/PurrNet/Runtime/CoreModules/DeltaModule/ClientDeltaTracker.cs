using System;
using System.Collections.Generic;
using PurrNet.Packing;

namespace PurrNet.Modules
{
    internal abstract class ClientDeltaTracker : IDisposable
    {
        public uint lastConfirmedId;
        public uint oldestIdInHistory;
        private uint nextId = 1;

        public uint GenerateId()
        {
            return nextId++;
        }

        public abstract void CleanupHistory(uint lastConfirmedId);

        public abstract bool ContainsKey(uint id);

        public abstract void Dispose();
    }

    internal class ClientDeltaTracker<T> : ClientDeltaTracker
    {
        private struct Entry
        {
            public uint key;
            public T value;
        }

        private readonly List<Entry> _history = new();

        private int BinarySearch(uint key)
        {
            int low = 0;
            int high = _history.Count - 1;

            while (low <= high)
            {
                int mid = (low + high) / 2;
                if (_history[mid].key < key)
                {
                    low = mid + 1;
                }
                else if (_history[mid].key > key)
                {
                    high = mid - 1;
                }
                else
                {
                    return mid;
                }
            }

            return low;
        }

        public override void CleanupHistory(uint lastConfirmedId)
        {
            int removeUpTo = BinarySearch(lastConfirmedId);

            if (removeUpTo == 0)
                return;

            if (removeUpTo >= _history.Count)
            {
                for (int i = 0; i < _history.Count; i++)
                {
                    if (_history[i].value is IDisposable disposable)
                        disposable.Dispose();
                }

                _history.Clear();
                return;
            }

            for (int i = 0; i < removeUpTo; i++)
            {
                if (_history[i].value is IDisposable disposable)
                    disposable.Dispose();
            }

            _history.RemoveRange(0, removeUpTo);
        }

        public override bool ContainsKey(uint id)
        {
            int index = BinarySearch(id);
            return index < _history.Count && _history[index].key == id;
        }

        public override void Dispose()
        {
            if (_history != null)
            {
                int c = _history.Count;
                for (int i = 0; i < c; i++)
                {
                    if (_history[i].value is IDisposable disposable)
                        disposable.Dispose();
                }

                _history.Clear();
            }
        }

        public bool TryGetValue(uint id, out T o)
        {
            int index = BinarySearch(id);

            if (index < _history.Count && _history[index].key == id)
            {
                o = _history[index].value;
                return true;
            }

            o = default;
            return false;
        }

        public void Set(uint id, T newValue)
        {
            int index = BinarySearch(id);
            if (index < _history.Count && _history[index].key == id)
            {
                var old = _history[index];
                if (old.value is IDisposable disposable)
                    disposable.Dispose();
                _history[index] = new Entry { key = id, value = Packer.Copy(newValue) };
            }
            else
            {
                _history.Insert(index, new Entry { key = id, value = Packer.Copy(newValue) });
            }
        }
    }
}
