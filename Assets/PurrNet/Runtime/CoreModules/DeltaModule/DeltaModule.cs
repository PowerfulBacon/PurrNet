using System.Collections.Generic;
using PurrNet.Modules;
using PurrNet.Packing;
using PurrNet.Transports;
using UnityEngine;

namespace PurrNet
{
    public class DeltaModule : INetworkModule
    {
        private NetworkManager _networkManager;
        private bool _asServer;
        
        private Dictionary<PlayerID, Dictionary<int, ClientDeltaTracker>> _clientTrackers;
        
        public DeltaModule(NetworkManager networkManager, bool asServer)
        {
            _networkManager = networkManager;
            _asServer = asServer;
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
            public object CurrentValue;
            public uint LastConfirmedId;
            public uint OldestIdInHistory;
            public Dictionary<uint, object> History;
            public uint NextId;
    
            public uint GenerateId()
            {
                return NextId++;
            }
    
            public void CleanupHistory(uint lastConfirmedId)
            {
                List<uint> toRemove = new List<uint>();
                foreach (var id in History.Keys)
                {
                    if (id < lastConfirmedId)
                        toRemove.Add(id);
                }
        
                foreach (var id in toRemove)
                    History.Remove(id);
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
                    CurrentValue = default(T),
                    LastConfirmedId = default,
                    History = new Dictionary<uint, object>(),
                    NextId = 1
                };
                clientDict[key] = tracker;
            }
            
            return tracker;
        }
        
        public bool Write<T>(BitPacker packer, PlayerID player, int key, T newValue)
        {
            var tracker = GetOrCreateTracker<T>(player, key);

            T oldValue = default;
            if (tracker.LastConfirmedId != 0 && tracker.History.TryGetValue(tracker.LastConfirmedId, out var confirmedValue))
            {
                oldValue = (T)confirmedValue;
            }

            Packer<uint>.Write(packer, tracker.LastConfirmedId);
    
            var pos = packer.positionInBits;
            Packer<bool>.Write(packer, false);
            bool changed = DeltaPacker<T>.Write(packer, oldValue, newValue);
            packer.WriteAt(pos, changed);
    
            //Debug.Log($"WRITE: Changed: {changed} | Player: {player} | LastConfirmedId: {tracker.LastConfirmedId} | Old: {oldValue} | New: {newValue}");

            if (changed)
            {
                PackedUInt newId = tracker.GenerateId();
                Packer<PackedUInt>.Write(packer, newId);

                tracker.CurrentValue = newValue;
                tracker.History[newId] = newValue;

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
                if (lastConfirmedId != 0 && tracker.History.TryGetValue(lastConfirmedId, out var confirmedValue))
                {
                    oldValue = (T)confirmedValue;
                }
        
                DeltaPacker<T>.Read(packer, oldValue, ref newValue);
                Packer<PackedUInt>.Read(packer, ref valueId);

                tracker.CurrentValue = newValue;
                tracker.History[valueId] = newValue;
        
                CleanupClientHistory(ref tracker);

                var clientDict = _clientTrackers[player];
                clientDict[key] = tracker;
                //Debug.Log($"NEW value for {player} - key: {key} - {newValue} (based on confirmed ID: {lastConfirmedId})");
            }
            else
            {
                newValue = tracker.CurrentValue != null 
                    ? (T)tracker.CurrentValue 
                    : default;
        
                //Debug.Log($"CACHED value for {player} - key: {key} - {newValue}");
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
            if (tracker.History.Count <= maxHistoryCount)
                return;
    
            if (tracker.OldestIdInHistory == 0 && tracker.History.Count > 0)
            {
                uint minId = uint.MaxValue;
                foreach (var id in tracker.History.Keys)
                {
                    if (id < minId) minId = id;
                }
                tracker.OldestIdInHistory = minId;
            }
    
            int toRemoveCount = tracker.History.Count - maxHistoryCount;
    
            for (int i = 0; i < toRemoveCount; i++)
            {
                tracker.History.Remove(tracker.OldestIdInHistory);
                tracker.OldestIdInHistory++;
            }
        }
        
        private void Acknowledge(PlayerID player, DeltaAcknowledge data, bool asServer)
        {
            if (!_clientTrackers.TryGetValue(player, out var clientDict) || 
                !clientDict.TryGetValue(data.key, out var tracker))
                return;
    
            if (tracker.History.ContainsKey(data.valueId))
            {
                if (data.valueId > tracker.LastConfirmedId)
                {
                    tracker.LastConfirmedId = data.valueId;
                    tracker.CleanupHistory(data.valueId);
            
                    clientDict[data.key] = tracker;
            
                    //Debug.Log($"{player} acknowledged value {valueId.value} for key {key}");
                }
            }
        }
    }
}