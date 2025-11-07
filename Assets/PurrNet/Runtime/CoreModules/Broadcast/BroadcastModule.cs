using System;
using System.Collections.Generic;
using PurrNet.Logging;
using PurrNet.Packing;
using PurrNet.Profiler;
using PurrNet.Transports;
using PurrNet.Utils;

namespace PurrNet.Modules
{
    public delegate void BroadcastDelegate<in T>(Connection conn, T data, bool asServer);

    public interface IBroadcastCallback
    {
        bool IsSame(object callback);

        void TriggerCallback(Connection conn, object data, bool asServer);

        void Subscribe(BroadcastModule module);
    }

    internal readonly struct BroadcastCallback<T> : IBroadcastCallback
    {
        readonly BroadcastDelegate<T> callback;

        public BroadcastCallback(BroadcastDelegate<T> callback)
        {
            this.callback = callback;
        }

        public bool IsSame(object callbackToCmp)
        {
            return callbackToCmp is BroadcastDelegate<T> action && action == callback;
        }

        public void TriggerCallback(Connection conn, object data, bool asServer)
        {
            if (data is T value)
                callback?.Invoke(conn, value, asServer);
        }

        public void Subscribe(BroadcastModule module)
        {
            module.Subscribe(callback);
        }
    }

    public class BroadcastModule : INetworkModule, IDataListener, IConnectionListener
    {
        private readonly ITransport _transport;

        private readonly bool _asServer;

        private readonly Dictionary<uint, List<IBroadcastCallback>> _actions =
            new Dictionary<uint, List<IBroadcastCallback>>();

        internal event Action<Connection, uint, object> onRawDataReceived;

        public BroadcastModule(INetworkManager manager, bool asServer)
        {
            _transport = manager.rawTransport;
            _asServer = asServer;
        }

        public void Enable(bool asServer)
        {
        }

        public void Disable(bool asServer)
        {
        }

        void AssertIsServer(string message)
        {
            if (!_asServer)
                throw new InvalidOperationException(PurrLogger.FormatMessage(message));
        }

        private readonly Dictionary<Connection, PackedUInt> _lastReadReliableId = new ();
        private readonly Dictionary<Connection, PackedUInt> _lastWrittenReliableId = new ();

        private PackedUInt GetLastWrittenReliableId(Connection conn)
        {
            return _lastWrittenReliableId.GetValueOrDefault(conn);
        }

        public PackedUInt GetLastReadReliableId(Connection conn)
        {
            return _lastReadReliableId.GetValueOrDefault(conn);
        }

        public static ByteData GetImmediateData(object data)
        {
            using var stream = BitPackerPool.Get();
            Packer<bool>.Write(stream, false);
            Packer<PackedUInt>.Write(stream, Hasher.GetStableHashU32(data.GetType()));
            Packer.Write(stream, data);
            return stream.ToByteData();
        }

        private ByteData GetData<T>(Connection conn, T data, Channel channel)
        {
            bool isReliable = channel == Channel.ReliableOrdered;

            using var stream = BitPackerPool.Get();
            var typeId = Hasher.GetStableHashU32<T>();

            Packer<bool>.Write(stream, isReliable);

            if (isReliable)
            {
                var old = GetLastWrittenReliableId(conn);
                DeltaPacker<PackedUInt>.Write(stream, old, typeId);
                _lastWrittenReliableId[conn] = typeId;
            }
            else
            {
                Packer<PackedUInt>.Write(stream, typeId);
            }

            Packer<T>.Write(stream, data);
            return stream.ToByteData();
        }

        static bool ShouldTrackType(Type type)
        {
            return type != typeof(RPCPacket) && type != typeof(ChildRPCPacket) && type != typeof(StaticRPCPacket);
        }

        public void SendToAll<T>(T data, Channel method = Channel.ReliableOrdered)
        {
            AssertIsServer("Cannot send data to all clients from client.");

#if UNITY_EDITOR || PURR_RUNTIME_PROFILING
            var type = typeof(T);
            bool shouldTrack = ShouldTrackType(type);
#endif
            int connCount = _transport.connections.Count;
            for (int i = 0; i < connCount; i++)
            {
                var conn = _transport.connections[i];
                var byteData = GetData(conn, data, method);

#if UNITY_EDITOR || PURR_RUNTIME_PROFILING
                if (shouldTrack)
                    Statistics.SentBroadcast(type, byteData.segment);
#endif
                _transport.SendToClient(_transport.connections[i], byteData, method);
            }
        }

        public void SendRaw(Connection conn, ByteData data, Channel method = Channel.ReliableOrdered)
        {
            AssertIsServer("Cannot send data to player from client.");
#if UNITY_EDITOR || PURR_RUNTIME_PROFILING
            Statistics.ForwardedBytes(data.length);
#endif
            _transport.SendToClient(conn, data, method);
        }

        public void Send<T>(Connection conn, T data, Channel method = Channel.ReliableOrdered)
        {
            AssertIsServer("Cannot send data to player from client.");

            var byteData = GetData(conn, data, method);
#if UNITY_EDITOR || PURR_RUNTIME_PROFILING
            var type = typeof(T);
            if (ShouldTrackType(type))
                Statistics.SentBroadcast(type, byteData.segment);
#endif
            _transport.SendToClient(conn, byteData, method);
        }

        public void Send<T>(IReadOnlyList<Connection> conn, T data, Channel method = Channel.ReliableOrdered)
        {
            AssertIsServer("Cannot send data to player from client.");

#if UNITY_EDITOR || PURR_RUNTIME_PROFILING
            var type = typeof(T);
            var shouldTrack = ShouldTrackType(type);
#endif

            for (var i = 0; i < conn.Count; i++)
            {
                var connection = conn[i];
                var byteData = GetData(connection, data, method);

#if UNITY_EDITOR || PURR_RUNTIME_PROFILING
                if (shouldTrack)
                    Statistics.SentBroadcast(type, byteData.segment);
#endif
                _transport.SendToClient(connection, byteData, method);
            }
        }

        public void Send(IReadOnlyList<Connection> conn, ByteData byteData, Channel method = Channel.ReliableOrdered)
        {
            AssertIsServer("Cannot send data to player from client.");

            for (var i = 0; i < conn.Count; i++)
            {
                var connection = conn[i];
#if UNITY_EDITOR || PURR_RUNTIME_PROFILING
                Statistics.ForwardedBytes(byteData.length);
#endif
                _transport.SendToClient(connection, byteData, method);
            }
        }

        public void SendToServer<T>(T data, Channel method = Channel.ReliableOrdered)
        {
            if (_asServer)
                return;

            var byteData = GetData(default, data, method);

#if UNITY_EDITOR || PURR_RUNTIME_PROFILING
            var type = typeof(T);
            if (ShouldTrackType(type))
                Statistics.SentBroadcast(type, byteData.segment);
#endif
            _transport.SendToServer(byteData, method);
        }

        public void OnDataReceived(Connection conn, ByteData data, bool asServer)
        {
            if (_asServer != asServer)
                return;

            using var stream = BitPackerPool.Get(data);

            PackedUInt typeId = default;

            bool isReliable = Packer<bool>.Read(stream);

            if (isReliable)
            {
                var lastRead = GetLastReadReliableId(conn);
                DeltaPacker<PackedUInt>.Read(stream, lastRead, ref typeId);
                _lastReadReliableId[conn] = typeId;
            }
            else
            {
                Packer<PackedUInt>.Read(stream, ref typeId);
            }

            if (!Hasher.TryGetType(typeId, out var typeInfo))
            {
                PurrLogger.LogError(
                    $"Cannot find type with id {typeId}; type must not have been registered properly.\nData: {data.ToString()}");
                return;
            }

            object instance = null;
            Packer.Read(stream, typeInfo, ref instance);
            TriggerCallback(conn, typeId, instance);

#if UNITY_EDITOR || PURR_RUNTIME_PROFILING
            if (ShouldTrackType(typeInfo))
                Statistics.ReceivedBroadcast(typeInfo, data.segment);
#endif
        }

        public void Subscribe<T>(BroadcastDelegate<T> callback)
        {
            var hash = Hasher.GetStableHashU32(typeof(T));

            if (_actions.TryGetValue(hash, out var actions))
            {
                actions.Add(new BroadcastCallback<T>(callback));
                return;
            }

            _actions.Add(hash, new List<IBroadcastCallback>
            {
                new BroadcastCallback<T>(callback)
            });
        }

        public void Unsubscribe<T>(BroadcastDelegate<T> callback)
        {
            var hash = Hasher.GetStableHashU32(typeof(T));
            if (!_actions.TryGetValue(hash, out var actions))
                return;

            object boxed = callback;

            for (int i = 0; i < actions.Count; i++)
            {
                if (actions[i].IsSame(boxed))
                {
                    actions.RemoveAt(i);
                    return;
                }
            }
        }

        private void TriggerCallback(Connection conn, uint hash, object instance)
        {
            if (_actions.TryGetValue(hash, out var actions))
            {
                for (int i = 0; i < actions.Count; i++)
                    actions[i].TriggerCallback(conn, instance, _asServer);
            }

            onRawDataReceived?.Invoke(conn, hash, instance);
        }

        public void OnConnected(Connection conn, bool asServer)
        {
        }

        public void OnDisconnected(Connection conn, bool asServer)
        {
            _lastReadReliableId.Remove(conn);
            _lastWrittenReliableId.Remove(conn);
        }
    }
}
