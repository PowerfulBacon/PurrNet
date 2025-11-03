using System;
using JetBrains.Annotations;
using PurrNet.Packing;
using PurrNet.Transports;
using UnityEngine;

namespace PurrNet
{
    [Serializable]
    public class ValidatedSyncVar<T> : NetworkModule
    {
        [SerializeField] private SyncVar<T> _authoritative = new(default, 0f, false);

        private T _display;
        private bool _hasAuthoritative;
        private ulong _nextPacketId;
        private ulong _lastAppliedServerId;
        private ulong _pendingId;
        private bool _hasPending;

        public event Action<T> onChanged;
        public event Action<T, T> onChangedWithOld;

        public T value
        {
            get => _display;
            set
            {
                if (isServer)
                {
                    ApplyAuthoritative(value);
                    return;
                }

                var old = _display;
                if ((old == null && value == null) || (old != null && old.Equals(value))) return;

                _display = value;
                _pendingId = ++_nextPacketId;
                _hasPending = true;
                TriggerEvents(old, _display);

                using var pack = BitPackerPool.Get();
                Packer<T>.Write(pack, value);
                if (!owner.HasValue)
                {
                    Debug.LogError($"No owner found for validated syncvar in {parent.name}!", parent);
                    return;
                }
                
                SubmitCandidate(owner.Value, _pendingId, pack);
            }
        }
        
        private ValidatorDelegate _validator;
        private ValidationFailedDelegate _onFailed;
        
        public delegate bool ValidatorDelegate(T oldValue, T newValue);
        public delegate void ValidationFailedDelegate(T failedValue, T authoritativeValue);

        public ValidatedSyncVar(T initialValue = default)
        {
            _authoritative = new SyncVar<T>(initialValue, 0f, false);
            _display = initialValue;
        }

        public override void OnPoolReset()
        {
            onChanged = null;
            onChangedWithOld = null;
            _hasAuthoritative = false;
            _nextPacketId = 0;
            _lastAppliedServerId = 0;
            _pendingId = 0;
            _hasPending = false;
        }

        public override void OnEarlySpawn()
        {
            base.OnEarlySpawn();
            _authoritative.onChangedWithOld += OnAuthoritativeChanged;
        }

        public override void OnDespawned()
        {
            base.OnDespawned();
            _authoritative.onChangedWithOld -= OnAuthoritativeChanged;
        }

        private void OnAuthoritativeChanged(T oldAuth, T newAuth)
        {
            _hasAuthoritative = true;
            var old = _display;
            _display = newAuth;
            _hasPending = false;
            TriggerEvents(old, _display);
        }

        private void TriggerEvents(T oldValue, T newValue)
        {
            try { onChanged?.Invoke(newValue); } catch (Exception e) { Debug.LogException(e); }
            try { onChangedWithOld?.Invoke(oldValue, newValue); } catch (Exception e) { Debug.LogException(e); }
        }

        private void ApplyAuthoritative(T v)
        {
            var old = _authoritative.value;
            _authoritative.value = v;
            if (!_hasAuthoritative)
            {
                _hasAuthoritative = true;
                _display = v;
                TriggerEvents(old, v);
            }
        }

        [ServerRpc(Channel.ReliableOrdered, requireOwnership: true)]
        private void SubmitCandidate(PlayerID sender, PackedULong packetId, BitPacker candidate)
        {
            using (candidate)
            {
                if (!isServer) return;
                if (packetId <= _lastAppliedServerId) return;

                T proposed = default;
                Packer<T>.Read(candidate, ref proposed);
                var current = _authoritative.value;

                if (_validator != null && !_validator(current, proposed))
                {
                    using var rej = BitPackerPool.Get();
                    Packer<T>.Write(rej, current);
                    Packer<T>.Write(rej, proposed);
                    RejectOwner(sender, packetId, rej);
                    return;
                }

                _lastAppliedServerId = packetId;
                ApplyAuthoritative(proposed);

                using var ack = BitPackerPool.Get();
                Packer<T>.Write(ack, proposed);
                AcceptOwner(sender, packetId, ack);
            }
        }

        [TargetRpc(Channel.ReliableOrdered)]
        private void AcceptOwner(PlayerID target, PackedULong packetId, BitPacker payload)
        {
            using (payload)
            {
                if (isServer) return;
                if (!_hasPending || packetId != _pendingId) return;
                _hasPending = false;

                T v = default;
                Packer<T>.Read(payload, ref v);
                var old = _display;
                _display = v;
                TriggerEvents(old, v);
            }
        }

        [TargetRpc(Channel.ReliableOrdered)]
        private void RejectOwner(PlayerID target, PackedULong packetId, BitPacker payload)
        {
            using (payload)
            {
                if (isServer) return;
                if (!_hasPending || packetId != _pendingId) return;
                _hasPending = false;

                T authoritativeNow = default;
                T failed = default;
                Packer<T>.Read(payload, ref authoritativeNow);
                Packer<T>.Read(payload, ref failed);

                var old = _display;
                _display = authoritativeNow;
                TriggerEvents(old, _display);
                _onFailed?.Invoke(failed, authoritativeNow);
            }
        }

    }
}
