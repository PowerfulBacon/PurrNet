using System.Collections.Generic;
using PurrNet.Packing;
using PurrNet.Pooling;

namespace PurrNet.Modules
{
    internal abstract class ClientDeltaTracker
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
                history.Remove(id);

            ListPool<uint>.Destroy(toRemove);
        }

        public override bool ContainsKey(uint id)
        {
            return history.ContainsKey(id);
        }
    }
}
