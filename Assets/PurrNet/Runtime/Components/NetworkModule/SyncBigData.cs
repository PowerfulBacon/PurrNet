using System;
using System.Collections.Generic;
using K4os.Compression.LZ4;
using PurrNet.Logging;
using PurrNet.Packing;
using PurrNet.Pooling;
using PurrNet.Transports;
using PurrNet.Utils;
using UnityEngine;

namespace PurrNet
{
    internal struct BigDataState
    {
        public PlayerID player;
        public int sentPartsCount;
        public DisposableList<int> confirmedParts;
        public DisposableList<int> requestedParts;
    }

    internal struct BigDataReceiveState
    {
        public int totalParts;
        public int totalLength;
        public float timeSinceLastReceivedPart;
        public DisposableList<int> confirmedParts;
    }

    [Serializable]
    public struct SyncStatus
    {
        public float percent;
        public bool isDone;

        public bool Equals(SyncStatus other)
        {
            return Mathf.Approximately(percent, other.percent) && isDone == other.isDone;
        }
    }

    [Serializable]
    public class SyncBigData : NetworkModule, ITick
    {
        [SerializeField, PurrLock] private bool _ownerAuth;
        [SerializeField, Min(1)] private int _maxKBPerSec;
        [SerializeField, HideInInspector] private SyncStatus _syncStatus;

        private List<BigDataState> _pending = new ();

        public SyncStatus syncStatus
        {
            get => _syncStatus;
            private set => _syncStatus = value;
        }
        private BigDataReceiveState _receivingState;

        public event Action<SyncStatus> onSyncStatusChanged;
        public ReadOnlySpan<byte> data => _data;

        private byte[] _data;
        private const int PART_SIZE = 1000;
        private int _totalParts;

        public int maxKBPerSec
        {
            get => _maxKBPerSec;
            set => _maxKBPerSec = Mathf.Max(1, value);
        }

        public SyncBigData(bool ownerAuth = false, int maxKBPerSec = 15)
        {
            _ownerAuth = ownerAuth;
            _maxKBPerSec = Mathf.Max(1, maxKBPerSec);
        }

        public override void OnSpawn()
        {
            ReQueueEveryone();
        }

        public void SetData(ReadOnlySpan<byte> data)
        {
            if (!isSpawned)
            {
                PurrLogger.LogError($"Trying to set data on `<b>{GetType().Name} {name}</b>` which is not spawned.", parent);
                return;
            }

            if (!IsController(_ownerAuth))
            {
                PurrLogger.LogError(
                    $"Invalid permissions when setting `<b>{GetType().Name} {name}</b>` on `{parent.name}`." +
                    $"\n{GetPermissionErrorDetails(_ownerAuth, this)}", parent);
                return;
            }

            if (!data.IsEmpty)
            {
                _data = LZ4Pickler.Pickle(data, LZ4Level.L12_MAX);
                _totalParts = (int)Math.Ceiling(data.Length / (double)PART_SIZE);
            }
            else
            {
                _data = default;
                _totalParts = 0;
            }

            ReQueueEveryone();
        }

        public override void OnObserverAdded(PlayerID player)
        {
            if (_data == null || _data.Length == 0)
                return;

            if (player == localPlayer)
                return;

            _pending ??= new List<BigDataState>();
            _pending.Add(new BigDataState
            {
                player = player,
                confirmedParts = DisposableList<int>.Create(),
                requestedParts = DisposableList<int>.Create()
            });
        }

        public override void OnObserverRemoved(PlayerID player)
        {
            _pending?.RemoveAll(v => v.player == player);
        }

        private void ReQueueEveryone()
        {
            if (_data == null || _data.Length == 0)
                return;

            _pending ??= new List<BigDataState>();
            _pending.Clear();

            if (isServer)
            {
                for (var i = parent.observers.Count - 1; i >= 0; i--)
                {
                    var observer = parent.observers[i];

                    if (observer == localPlayer || (_ownerAuth && observer == owner))
                        continue;

                    _pending.Add(new BigDataState
                    {
                        player = observer,
                        confirmedParts = DisposableList<int>.Create(),
                        requestedParts = DisposableList<int>.Create()
                    });
                }
            }
            else if (IsController(_ownerAuth))
            {
                _pending.Add(new BigDataState
                {
                    player = PlayerID.Server,
                    confirmedParts = DisposableList<int>.Create(),
                    requestedParts = DisposableList<int>.Create()
                });
            }
        }

        private ByteData GetDataPart(int part)
        {
            if (_data == null) return default;

            if (part >= _totalParts)
                return default;

            if (part == _totalParts - 1)
            {
                return new ByteData(_data, part * PART_SIZE,
                    _data.Length - part * PART_SIZE);
            }

            return new ByteData(_data, part * PART_SIZE, PART_SIZE);
        }

        private float _partsCounter;

        public void OnTick(float delta)
        {
            if (_syncStatus is { percent: > 0, isDone: false })
            {
                float timeSinceLastPart = Time.unscaledTime - _receivingState.timeSinceLastReceivedPart;
                float expectedPerTick = _maxKBPerSec * delta;
                float minTimeToPart = 1f / expectedPerTick;

                if (timeSinceLastPart > minTimeToPart * 3)
                {
                    _receivingState.timeSinceLastReceivedPart = Time.unscaledTime;
                    RequestMissingParts();
                }
            }

            if (_pending == null || _pending.Count == 0)
                return;

            _partsCounter += _maxKBPerSec * delta;

            if (_partsCounter < 1)
                return;

            for (int i = _pending.Count - 1; i >= 0; i--)
            {
                var state = _pending[i];

                bool isFirst = state.sentPartsCount == 0;

                if (isFirst)
                {
                    SendDownloadStart(ref state);
                    _pending[i] = state;
                    continue;
                }

                bool hasFirstPart = state.confirmedParts.Count > 0;

                if (!hasFirstPart)
                    continue;

                int partsBudget = (int)_partsCounter;
                SendNewParts(ref partsBudget, ref state);
                SendRequestedParts(partsBudget, state);
                _pending[i] = state;
            }

            _partsCounter = Mathf.Max(0, _partsCounter - (int)_partsCounter);
        }

        private void RequestMissingParts()
        {
            var parts = DisposableList<Size>.Create();
            for (int i = 1; i < _receivingState.confirmedParts.Count; i++)
            {
                var prev = _receivingState.confirmedParts[i - 1];
                var curr = _receivingState.confirmedParts[i];

                for (int j = prev + 1; j < curr; j++)
                    parts.Add(j);
            }

            int lastConfirmed = _receivingState.confirmedParts[^1];
            if (lastConfirmed < _totalParts - 1)
            {
                for (int j = lastConfirmed + 1; j < _totalParts; j++)
                    parts.Add(j);
            }

            const int MAX_ENTRIES = 150;

            if (parts.Count > MAX_ENTRIES)
            {
                int chunks = parts.Count / MAX_ENTRIES;

                for (int i = 0; i < chunks; i++)
                {
                    using var chunk = DisposableList<Size>.Create();
                    int start = i * MAX_ENTRIES;
                    for (int j = 0; j < MAX_ENTRIES; j++)
                        chunk.Add(parts[start + j]);
                    RequestMissingParts(chunk);
                }

                parts.RemoveRange(chunks * MAX_ENTRIES, parts.Count - chunks * MAX_ENTRIES);
            }

            if (parts.Count > 0)
                RequestMissingParts(parts);
        }

        [ServerRpc(Channel.Unreliable)]
        private void RequestMissingParts(DisposableList<Size> parts, RPCInfo info = default)
        {
            using (parts)
            {
                for (int i = _pending.Count - 1; i >= 0; i--)
                {
                    var v = _pending[i];
                    if (v.player == info.sender)
                    {
                        for (var p = 0; p < parts.Count; p++)
                        {
                            var part = parts[p];
                            if (!v.requestedParts.Contains(part))
                                v.requestedParts.Add(part);
                        }
                        break;
                    }
                }
            }
        }

        private void SendRequestedParts(int partsBudget, BigDataState state)
        {
            if (partsBudget > 0)
            {
                for (int j = 0; j < state.requestedParts.Count; j++)
                {
                    if (partsBudget <= 0)
                        break;

                    var part = GetDataPart(state.requestedParts[j]);
                    SendPartToTarget(state.player, part, state.requestedParts[j]);
                    state.requestedParts.RemoveAt(j--);
                    --partsBudget;
                }
            }
        }

        private void SendNewParts(ref int partsBudget, ref BigDataState state)
        {
            for (int j = partsBudget; j >= 0; --j)
            {
                if (state.sentPartsCount < _totalParts)
                {
                    var part = GetDataPart(state.sentPartsCount);
                    SendPartToTarget(state.player, part, state.sentPartsCount);
                    ++state.sentPartsCount;
                    --partsBudget;
                }
                else break;
            }
        }

        private void SendDownloadStart(ref BigDataState state)
        {
            SendFirstPart(state.player, GetDataPart(0), _totalParts, _data.Length);
            state.sentPartsCount++;
        }

        [TargetRpc]
        private void SendFirstPart(PlayerID player, ByteData data, int totalParts, int totalLength)
        {
            _receivingState = new BigDataReceiveState
            {
                totalParts = totalParts,
                totalLength = totalLength,
                confirmedParts = DisposableList<int>.Create()
            };

            _totalParts = totalParts;

            if (_data == null)
                _data = new byte[totalLength];
            else if (_data.Length != totalLength)
                Array.Resize(ref _data, totalLength);

            InsertConfirmedPart(data, 0);
            ConfirmFirstPart();
        }

        [TargetRpc(channel: Channel.Unreliable)]
        private void SendPartToTarget(PlayerID player, ByteData data, int partId)
        {
            InsertConfirmedPart(data, partId);
        }

        private void InsertConfirmedPart(ByteData data, int partId)
        {
            int toInsert = _receivingState.confirmedParts.Count;
            for (int i = 0; i < toInsert; i++)
            {
                if (_receivingState.confirmedParts[i] > partId)
                {
                    toInsert = i;
                    break;
                }

                // If we already have this part, we're done'
                if (_receivingState.confirmedParts[i] == partId) return;
            }

            _receivingState.confirmedParts.Insert(toInsert, partId);
            _receivingState.timeSinceLastReceivedPart = Time.unscaledTime;

            int partSize = Mathf.Min(PART_SIZE, _receivingState.totalLength - partId * PART_SIZE);
            Array.Copy(data.data, data.offset, _data, partId * PART_SIZE, partSize);

            syncStatus = new SyncStatus
            {
                percent = _receivingState.confirmedParts.Count / (float)_receivingState.totalParts,
                isDone = _receivingState.confirmedParts.Count == _receivingState.totalParts
            };
            onSyncStatusChanged?.Invoke(syncStatus);
        }

        delegate void ModifyEntry(ref BigDataState entry);

        private bool ModifyState(PlayerID player, ModifyEntry modify)
        {
            for (int i = _pending.Count - 1; i >= 0; i--)
            {
                var v = _pending[i];
                if (v.player == player)
                {
                    modify(ref v);
                    _pending[i] = v;
                    return true;
                }
            }

            return false;
        }

        [ServerRpc(requireOwnership: false)]
        private void ConfirmFirstPart(RPCInfo info = default)
        {
            if (!ModifyState(info.sender, ConfirmFirstEntry))
            {
                PurrLogger.LogError($"Failed to confirm first part for player {info.sender} for " +
                                    $"`<b>{GetType().Name} {name}</b>` on `{parent.name}`", parent);
            }
        }

        static void ConfirmFirstEntry(ref BigDataState entry)
        {
            entry.confirmedParts.Add(0);
        }
    }
}
