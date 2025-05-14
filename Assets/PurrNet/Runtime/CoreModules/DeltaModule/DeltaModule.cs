using System.Collections.Generic;
using PurrNet.Modules;
using PurrNet.Packing;
using PurrNet.Pooling;
using PurrNet.Transports;

namespace PurrNet
{
    public class DeltaModule : INetworkModule
    {
        private readonly NetworkManager _networkManager;
        private readonly Dictionary<PlayerID, Dictionary<int, ClientDeltaTracker>> _clientTrackers;

        public DeltaModule(NetworkManager networkManager)
        {
            _networkManager = networkManager;
            _clientTrackers = new Dictionary<PlayerID, Dictionary<int, ClientDeltaTracker>>();
        }

        public void Enable(bool asServer)
        {
            _networkManager.Subscribe<DeltaAcknowledge>(Acknowledge, asServer);
        }

        public void Disable(bool asServer)
        {
            _clientTrackers.Clear();
            _networkManager.Unsubscribe<DeltaAcknowledge>(Acknowledge, asServer);
        }

        private struct DeltaAcknowledge : IPackedAuto
        {
            public int key;
            public PackedUInt valueId;
        }

        private struct ClientDeltaTracker
        {
            public object currentValue;
            public uint lastConfirmedId;
            public uint oldestIdInHistory;
            public Dictionary<uint, object> history;
            public uint nextId;

            public uint GenerateId()
            {
                return nextId++;
            }

            public void CleanupHistory(uint lastConfirmedId)
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
        }

        private ClientDeltaTracker GetOrCreateTracker<T>(PlayerID player, int key)
        {
            if (!_clientTrackers.TryGetValue(player, out var clientDict))
            {
                clientDict = new Dictionary<int, ClientDeltaTracker>();
                _clientTrackers[player] = clientDict;
            }

            if (!clientDict.TryGetValue(key, out var tracker))
            {
                tracker = new ClientDeltaTracker
                {
                    currentValue = default(T),
                    lastConfirmedId = default,
                    history = new Dictionary<uint, object>(),
                    nextId = 1
                };
                clientDict[key] = tracker;
            }

            return tracker;
        }

        public bool Write<T>(BitPacker packer, PlayerID player, int key, T newValue)
        {
            var tracker = GetOrCreateTracker<T>(player, key);

            T oldValue = default;
            if (tracker.lastConfirmedId != 0 && tracker.history.TryGetValue(tracker.lastConfirmedId, out var confirmedValue))
            {
                oldValue = (T)confirmedValue;
            }

            Packer<uint>.Write(packer, tracker.lastConfirmedId);

            var pos = packer.positionInBits;
            Packer<bool>.Write(packer, false);
            bool changed = DeltaPacker<T>.Write(packer, oldValue, newValue);
            packer.WriteAt(pos, changed);

            //Debug.Log($"WRITE: Changed: {changed} | Player: {player} | LastConfirmedId: {tracker.LastConfirmedId} | Old: {oldValue} | New: {newValue}");

            if (changed)
            {
                PackedUInt newId = tracker.GenerateId();
                Packer<PackedUInt>.Write(packer, newId);

                tracker.currentValue = newValue;
                tracker.history[newId] = newValue;

                var clientDict = _clientTrackers[player];
                clientDict[key] = tracker;
            }
            else
            {
                packer.SetBitPosition(pos + 1);
            }

            return changed;
        }

        public void Read<T>(BitPacker packer, PlayerID player, int key, ref T newValue, ref PackedUInt valueId)
        {
            var tracker = GetOrCreateTracker<T>(player, key);

            uint lastConfirmedId = default;
            Packer<uint>.Read(packer, ref lastConfirmedId);

            bool changed = false;
            Packer<bool>.Read(packer, ref changed);

            if (changed)
            {
                T oldValue = default;
                if (lastConfirmedId != 0 && tracker.history.TryGetValue(lastConfirmedId, out var confirmedValue))
                {
                    oldValue = (T)confirmedValue;
                }

                DeltaPacker<T>.Read(packer, oldValue, ref newValue);
                Packer<PackedUInt>.Read(packer, ref valueId);

                tracker.currentValue = newValue;
                tracker.history[valueId] = newValue;

                CleanupClientHistory(ref tracker);

                var clientDict = _clientTrackers[player];
                clientDict[key] = tracker;
            }
            else
            {
                newValue = tracker.currentValue != null
                    ? (T)tracker.currentValue
                    : default;
            }

            var data = new DeltaAcknowledge
            {
                key = key,
                valueId = valueId
            };
            _networkManager.SendToServer(data, Channel.Unreliable);
        }

        private void CleanupClientHistory(ref ClientDeltaTracker tracker, int maxHistoryCount = 10)
        {
            if (tracker.history.Count <= maxHistoryCount)
                return;

            if (tracker.oldestIdInHistory == 0 && tracker.history.Count > 0)
            {
                uint minId = uint.MaxValue;
                foreach (var id in tracker.history.Keys)
                {
                    if (id < minId) minId = id;
                }
                tracker.oldestIdInHistory = minId;
            }

            int toRemoveCount = tracker.history.Count - maxHistoryCount;

            for (int i = 0; i < toRemoveCount; i++)
            {
                tracker.history.Remove(tracker.oldestIdInHistory);
                tracker.oldestIdInHistory++;
            }
        }

        private void Acknowledge(PlayerID player, DeltaAcknowledge data, bool asServer)
        {
            if (!_clientTrackers.TryGetValue(player, out var clientDict) ||
                !clientDict.TryGetValue(data.key, out var tracker))
                return;

            if (tracker.history.ContainsKey(data.valueId))
            {
                if (data.valueId > tracker.lastConfirmedId)
                {
                    tracker.lastConfirmedId = data.valueId;
                    tracker.CleanupHistory(data.valueId);

                    clientDict[data.key] = tracker;

                    //Debug.Log($"{player} acknowledged value {valueId.value} for key {key}");
                }
            }
        }
    }
}
