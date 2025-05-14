using System.Collections.Generic;
using PurrNet.Packing;
using PurrNet.Transports;

namespace PurrNet.Modules
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

        private ClientDeltaTracker<T> GetOrCreateTracker<T>(PlayerID player, int key)
        {
            if (!_clientTrackers.TryGetValue(player, out var clientDict))
            {
                clientDict = new Dictionary<int, ClientDeltaTracker>();
                _clientTrackers[player] = clientDict;
            }

            if (!clientDict.TryGetValue(key, out var tracker))
            {
                var result = new ClientDeltaTracker<T>();
                tracker = result;
                clientDict[key] = tracker;
                return result;
            }

            if (tracker is not ClientDeltaTracker<T> typedTracker)
                throw new System.Exception($"Tracker for key {key} is not of type {typeof(ClientDeltaTracker<T>).Name}");

            return typedTracker;
        }

        public bool Write<T>(BitPacker packer, PlayerID player, int key, T newValue)
        {
            var tracker = GetOrCreateTracker<T>(player, key);

            T oldValue = default;

            if (tracker.lastConfirmedId != 0 && tracker.history.TryGetValue(tracker.lastConfirmedId, out var confirmedValue))
                oldValue = confirmedValue;

            Packer<PackedUInt>.Write(packer, tracker.lastConfirmedId);

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

            PackedUInt lastConfirmedId = default;
            Packer<PackedUInt>.Read(packer, ref lastConfirmedId);

            bool changed = false;
            Packer<bool>.Read(packer, ref changed);

            if (changed)
            {
                T oldValue = default;
                if (lastConfirmedId != 0 && tracker.history.TryGetValue(lastConfirmedId, out var confirmedValue))
                {
                    oldValue = confirmedValue;
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
                    ? tracker.currentValue
                    : default;
            }

            var data = new DeltaAcknowledge
            {
                key = key,
                valueId = valueId
            };
            _networkManager.SendToServer(data, Channel.Unreliable);
        }

        private static void CleanupClientHistory<T>(ref ClientDeltaTracker<T> tracker, int maxHistoryCount = 10)
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

            if (tracker.ContainsKey(data.valueId))
            {
                if (data.valueId > tracker.lastConfirmedId)
                {
                    tracker.lastConfirmedId = data.valueId;
                    tracker.CleanupHistory(data.valueId);

                    clientDict[data.key] = tracker;
                }
            }
        }
    }
}
