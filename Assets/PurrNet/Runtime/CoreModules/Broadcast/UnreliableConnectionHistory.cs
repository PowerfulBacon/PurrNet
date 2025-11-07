using System;
using System.Collections.Generic;
using PurrNet.Packing;
using PurrNet.Transports;

namespace PurrNet.Modules
{
    public struct UnreliableAck
    {
        public Connection conn;
        public uint ack; // store plain uint for simplicity
    }

    public class UnreliableConnectionHistory<T>
    {
        private readonly Dictionary<Connection, Dictionary<uint, T>> _history = new();
        private readonly Dictionary<Connection, uint> _lastAcked = new();
        private readonly Dictionary<Connection, uint> _lastSeqId = new();
        private readonly List<UnreliableAck> _pendingAcks = new();

        private uint NextSeqId(Connection conn)
        {
            if (!_lastSeqId.TryGetValue(conn, out var seq))
                seq = 0;
            seq++;
            _lastSeqId[conn] = seq;
            return seq;
        }

        private static void TrimHistory(Dictionary<uint, T> history, uint acked)
        {
            return;
            var toRemove = new List<uint>();
            foreach (var k in history.Keys)
            {
                // simple monotonic safety, ok for non-wrap cases
                if (k < acked)
                    toRemove.Add(k);
            }

            foreach (var k in toRemove)
                if (history.Remove(k, out var v) && v is IDisposable d)
                    d.Dispose();
        }

        public void WriteReliable(BitPacker stream, Connection conn, T newValue)
        {
            if (!_history.TryGetValue(conn, out var connHistory))
                _history[conn] = connHistory = new Dictionary<uint, T>();

            var seqId = NextSeqId(conn);
            var acked = _lastAcked.GetValueOrDefault(conn);
            T old = default;

            // try delta from last known ACK baseline
            if (acked > 0 && connHistory.TryGetValue(acked, out var ackedVal))
            {
                old = ackedVal;
            }

            Packer<PackedUInt>.Write(stream, acked);
            DeltaPacker<PackedUInt>.Write(stream, acked, seqId);
            DeltaPacker<T>.Write(stream, old, newValue);
            connHistory[seqId] = Packer.Copy(newValue);

            TrimHistory(connHistory, acked);
        }

        public T ReadReliable(BitPacker stream, Connection conn)
        {
            if (!_history.TryGetValue(conn, out var connHistory))
                _history[conn] = connHistory = new Dictionary<uint, T>();

            uint oldId = Packer<PackedUInt>.Read(stream);
            uint seqId = DeltaPacker<PackedUInt>.Read(stream, oldId);

            T old = default;

            if (connHistory.TryGetValue(oldId, out var existing))
            {
                old = existing;
            }

            T newValue = default;
            DeltaPacker<T>.Read(stream, old, ref newValue);
            connHistory[seqId] = Packer.Copy(newValue);
            RegisterAck(conn, seqId);

            TrimHistory(connHistory, seqId);
            return newValue;
        }

        public void ReceiveAcks(Connection conn, BitPacker stream)
        {
            if (!Packer<bool>.Read(stream))
                return;


            var ackSeqId = Packer<PackedUInt>.Read(stream).value;
            _lastAcked[conn] = ackSeqId;

            if (_history.TryGetValue(conn, out var hist))
                TrimHistory(hist, ackSeqId);
        }

        public bool SendAcks(Connection conn, BitPacker stream)
        {
            for (int i = _pendingAcks.Count - 1; i >= 0; i--)
            {
                var ack = _pendingAcks[i];
                if (ack.conn == conn)
                {
                    Packer<bool>.Write(stream, true);
                    Packer<PackedUInt>.Write(stream, ack.ack);
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
            if (_history.Remove(conn, out var hist))
            {
                foreach (var v in hist.Values)
                    if (v is IDisposable d)
                        d.Dispose();
            }
            _lastSeqId.Remove(conn);
            _lastAcked.Remove(conn);
        }
    }
}
