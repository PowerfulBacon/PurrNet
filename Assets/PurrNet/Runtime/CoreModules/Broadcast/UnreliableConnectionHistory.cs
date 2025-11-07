using System;
using System.Collections.Generic;
using PurrNet.Packing;
using PurrNet.Pooling;
using PurrNet.Transports;
using UnityEngine;

namespace PurrNet.Modules
{
    public struct UnreliableAck
    {
        public Connection conn;
        public uint ack;
    }

    public class UnreliableConnectionHistory<T>
    {
        // Each entry keeps its data and the time it was written
        private readonly struct Entry
        {
            public readonly T value;
            public readonly float time;
            public Entry(T v)
            {
                value = v;
                time = Time.realtimeSinceStartup;
            }
        }

        private readonly Dictionary<Connection, Dictionary<uint, Entry>> _history = new();
        private readonly Dictionary<Connection, uint> _lastAcked = new();
        private readonly Dictionary<Connection, uint> _lastSeqId = new();
        private readonly List<UnreliableAck> _pendingAcks = new();

        private const float ExpireAfter = 10f; // seconds
        private const int KeepMin = 4;

        private uint NextSeqId(Connection conn)
        {
            if (!_lastSeqId.TryGetValue(conn, out var seq))
                seq = 0;
            seq++;
            _lastSeqId[conn] = seq;
            return seq;
        }

        private static void CleanupOldEntries(Dictionary<uint, Entry> history)
        {
            float now = Time.realtimeSinceStartup;
            using var toRemove = DisposableList<uint>.Create(history.Count);

            foreach (var kvp in history)
            {
                if (history.Count - toRemove.Count > KeepMin &&
                    now - kvp.Value.time > ExpireAfter)
                    toRemove.Add(kvp.Key);
            }

            foreach (var k in toRemove)
                if (history.Remove(k, out var e) && e.value is IDisposable d)
                    d.Dispose();
        }

        public void WriteReliable(BitPacker stream, Connection conn, T newValue)
        {
            if (!_history.TryGetValue(conn, out var connHist))
                _history[conn] = connHist = new Dictionary<uint, Entry>();

            var seqId = NextSeqId(conn);
            var acked = _lastAcked.GetValueOrDefault(conn);
            T old = default;

            if (acked > 0 && connHist.TryGetValue(acked, out var ackedVal))
                old = ackedVal.value;

            Packer<PackedUInt>.Write(stream, acked);
            Packer<PackedUInt>.Write(stream, seqId);
            DeltaPacker<T>.Write(stream, old, newValue);

            connHist[seqId] = new Entry(Packer.Copy(newValue));
            CleanupOldEntries(connHist);
        }

        public T ReadReliable(BitPacker stream, Connection conn)
        {
            if (!_history.TryGetValue(conn, out var connHist))
                _history[conn] = connHist = new Dictionary<uint, Entry>();

            uint oldId = Packer<PackedUInt>.Read(stream);
            uint seqId = Packer<PackedUInt>.Read(stream);

            T old = default;
            if (connHist.TryGetValue(oldId, out var entry))
                old = entry.value;

            T newValue = default;
            DeltaPacker<T>.Read(stream, old, ref newValue);

            connHist[seqId] = new Entry(Packer.Copy(newValue));
            RegisterAck(conn, seqId);
            CleanupOldEntries(connHist);
            return newValue;
        }

        public void ReceiveAcks(Connection conn, BitPacker stream)
        {
            if (!Packer<bool>.Read(stream))
                return;

            var ackSeqId = Packer<PackedUInt>.Read(stream).value;
            _lastAcked[conn] = ackSeqId;
        }

        public bool SendAcks(Connection conn, BitPacker stream)
        {
            for (int i = _pendingAcks.Count - 1; i >= 0; i--)
            {
                var a = _pendingAcks[i];
                if (a.conn == conn)
                {
                    Packer<bool>.Write(stream, true);
                    Packer<PackedUInt>.Write(stream, a.ack);
                    _pendingAcks.RemoveAt(i);
                    return true;
                }
            }
            Packer<bool>.Write(stream, false);
            return false;
        }

        private void RegisterAck(Connection conn, uint seqId)
        {
            for (int i = _pendingAcks.Count - 1; i >= 0; i--)
            {
                if (_pendingAcks[i].conn == conn)
                {
                    _pendingAcks[i] = new UnreliableAck { conn = conn, ack = seqId };
                    return;
                }
            }
            _pendingAcks.Add(new UnreliableAck { conn = conn, ack = seqId });
        }

        public void Clear(Connection conn)
        {
            if (_history.Remove(conn, out var dict))
            {
                foreach (var e in dict.Values)
                    if (e.value is IDisposable d)
                        d.Dispose();
            }
            _lastSeqId.Remove(conn);
            _lastAcked.Remove(conn);
        }
    }
}
