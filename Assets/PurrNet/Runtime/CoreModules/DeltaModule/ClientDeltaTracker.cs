using System;
using System.Collections.Generic;
using PurrNet.Pooling;

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
        public readonly Dictionary<uint, T> history = new();

        public override void CleanupHistory(uint lastConfirmedId)
        {
            var toRemove = ListPool<uint>.Instantiate();

            foreach (var id in history.Keys)
            {
                if (id < lastConfirmedId)
                    toRemove.Add(id);
            }

            foreach (var id in toRemove)
            {
                if (history.Remove(id, out var item))
                {
                    if (item is IDisposable disposable)
                        disposable.Dispose();
                }
            }

            ListPool<uint>.Destroy(toRemove);
        }

        public override bool ContainsKey(uint id)
        {
            return history.ContainsKey(id);
        }

        public override void Dispose()
        {
            if (history != null)
            {
                foreach (var item in history.Values)
                {
                    if (item is IDisposable disposable)
                        disposable.Dispose();
                }

                history.Clear();
            }
        }
    }
}
