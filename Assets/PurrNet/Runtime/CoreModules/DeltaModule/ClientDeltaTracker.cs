using System;
using System.Collections.Generic;
using PurrNet.Packing;
using UnityEngine;

namespace PurrNet.Modules
{
    internal abstract class ClientDeltaTracker : IDisposable
    {
        private uint nextId = 1;

        public uint GenerateId()
        {
            return nextId++;
        }

        public abstract uint CleanupUpTo(float maxAge);

        public abstract void CleanupUpTo(uint exclusive);

        public abstract bool ContainsKey(uint id);

        public abstract void Dispose();
    }

    internal class ClientDeltaTracker<T> : ClientDeltaTracker
    {
        private struct Entry
        {
            public uint key;
            public T value;
            public float enterTime;
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

        private int BinarySearchOlderThan(float seconds)
        {
            float threshold = Time.unscaledTime - seconds;
            int low = 0;
            int high = _history.Count - 1;
            int result = _history.Count;

            while (low <= high)
            {
                int mid = (low + high) / 2;
                if (_history[mid].enterTime < threshold)
                {
                    result = mid;
                    high = mid - 1;
                }
                else
                {
                    low = mid + 1;
                }
            }

            return result;
        }

        public int FindBestMatch(in T value, out uint key)
        {
            int minBits = int.MaxValue;
            int bestMatchIndex = -1;
            key = 0;

            const int MAX_ITERATIONS = 5;

            int c = _history.Count;
            int minIndex = Math.Max(0, c - MAX_ITERATIONS);

            for (int i = c - 1; i >= minIndex; i--)
            {
                var entry = _history[i];
                int dist = DeltaPacker<T>.GetNecessaryBitsToWrite(entry.value, value);

                if (dist < minBits)
                {
                    minBits = dist;
                    bestMatchIndex = i;
                    key = entry.key;

                    if (minBits == 0)
                        break;
                }
            }

            return bestMatchIndex;
        }

        public override uint CleanupUpTo(float maxAge)
        {
            const int MAX_HISTORY_SIZE = 256;

            // If the history is smaller than the maximum size, we don't need to clean up.
            if (_history.Count < MAX_HISTORY_SIZE)
                return 0;

            if (_history.Count == 0)
                return 0;

            bool isFirstOldEnough = _history[0].enterTime < Time.unscaledTime - maxAge;

            if (!isFirstOldEnough)
                return 0;

            int removeUpTo = BinarySearchOlderThan(maxAge);

            for (int i = 0; i < removeUpTo; i++)
            {
                if (_history[i].value is IDisposable disposable)
                    disposable.Dispose();
            }

            _history.RemoveRange(0, removeUpTo);
            return _history.Count > 0 ? _history[0].key : 0;
        }

        public override void CleanupUpTo(uint exclusive)
        {
            int removeUpTo = BinarySearch(exclusive);

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

        public bool TryGetValueAtIndex(int id, out T o)
        {
            if (id < 0 || id >= _history.Count)
            {
                o = default;
                return false;
            }

            o = _history[id].value;
            return true;
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
