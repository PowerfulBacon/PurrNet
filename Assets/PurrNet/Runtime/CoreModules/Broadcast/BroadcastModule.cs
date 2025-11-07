using System;
using System.Collections.Generic;
using PurrNet.Logging;
using PurrNet.Packing;
using PurrNet.Profiler;
using PurrNet.Transports;
using PurrNet.Utils;

namespace PurrNet.Modules
{
    public class BroadcastModule : INetworkModule, IDataListener, IConnectionListener, IPostFixedUpdate
    {
        private readonly ITransport _transport;

        private readonly bool _asServer;

        private readonly Dictionary<uint, List<IBroadcastCallback>> _actions =
            new Dictionary<uint, List<IBroadcastCallback>>();

        internal event Action<Connection, uint, object> onRawDataReceived;

        private readonly ReliableConnectionHistory<PackedUInt> _reliableTypeCache = new ();
        private readonly ReliableConnectionHistory<ChildRPCPacket> _reliableChildRpcCache = new ();
        private readonly ReliableConnectionHistory<StaticRPCPacket> _reliableStaticRpcCache = new ();
        private readonly ReliableConnectionHistory<RPCPacket> _reliableRpcCache = new ();


        private readonly UnreliableConnectionHistory<PackedUInt> _unreliableTypeCache = new ();
        private readonly UnreliableConnectionHistory<ChildRPCPacket> _unreliableChildRpcCache = new ();
        private readonly UnreliableConnectionHistory<StaticRPCPacket> _unreliableStaticRpcCache = new ();
        private readonly UnreliableConnectionHistory<RPCPacket> _unreliableRpcCache = new ();

        public BroadcastModule(INetworkManager manager, bool asServer)
        {
            _transport = manager.rawTransport;
            _asServer = asServer;
        }

        public void Enable(bool asServer)
        {
            Subscribe<BroascastAcks>(OnAcks);
        }

        public void Disable(bool asServer)
        {
            Unsubscribe<BroascastAcks>(OnAcks);
        }

        void AssertIsServer(string message)
        {
            if (!_asServer)
                throw new InvalidOperationException(PurrLogger.FormatMessage(message));
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
                _reliableTypeCache.WriteReliable(stream, conn, typeId);
            }
            else
            {
                Packer<PackedUInt>.Write(stream, typeId);
            }

            switch (isReliable)
            {
                case true when data is RPCPacket packet:
                    _reliableRpcCache.WriteReliable(stream, conn, packet);
                    break;
                case false when data is RPCPacket packet:
                    _unreliableRpcCache.WriteReliable(stream, conn, packet);
                    break;
                case true when data is ChildRPCPacket child:
                    _reliableChildRpcCache.WriteReliable(stream, conn, child);
                    break;
                case false when data is ChildRPCPacket child:
                    _unreliableChildRpcCache.WriteReliable(stream, conn, child);
                    break;
                case true when data is StaticRPCPacket staticRpc:
                    _reliableStaticRpcCache.WriteReliable(stream, conn, staticRpc);
                    break;
                case false when data is StaticRPCPacket staticRpc:
                    _unreliableStaticRpcCache.WriteReliable(stream, conn, staticRpc);
                    break;
                default:
                    Packer<T>.Write(stream, data);
                    break;
            }

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

            var isReliable = Packer<bool>.Read(stream);
            var typeId = isReliable ?
                _reliableTypeCache.ReadReliable(stream, conn) :
                Packer<PackedUInt>.Read(stream);

            if (!Hasher.TryGetType(typeId, out var typeInfo))
            {
                PurrLogger.LogError(
                    $"Cannot find type with id {typeId}; type must not have been registered properly.\nData: {data.ToString()}");
                return;
            }

            object instance = null;

            switch (isReliable)
            {
                case true when typeInfo == typeof(RPCPacket):
                    instance = _reliableRpcCache.ReadReliable(stream, conn);
                    break;
                case false when typeInfo == typeof(RPCPacket):
                    instance = _unreliableRpcCache.ReadReliable(stream, conn);
                    break;
                case true when typeInfo == typeof(ChildRPCPacket):
                    instance = _reliableChildRpcCache.ReadReliable(stream, conn);
                    break;
                case false when typeInfo == typeof(ChildRPCPacket):
                    instance = _unreliableChildRpcCache.ReadReliable(stream, conn);
                    break;
                case true when typeInfo == typeof(StaticRPCPacket):
                    instance = _reliableStaticRpcCache.ReadReliable(stream, conn);
                    break;
                case false when typeInfo == typeof(StaticRPCPacket):
                    instance = _unreliableStaticRpcCache.ReadReliable(stream, conn);
                    break;
                default:
                    Packer.Read(stream, typeInfo, ref instance);
                    break;
            }

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

        public void PostFixedUpdate()
        {
            using var stream = BitPackerPool.Get();

            for (int i = _transport.connections.Count - 1; i >= 0; i--)
            {
                var conn = _transport.connections[i];
                bool any = _unreliableTypeCache.SendAcks(conn, stream);
                any = _unreliableRpcCache.SendAcks(conn, stream) || any;
                any = _unreliableChildRpcCache.SendAcks(conn, stream) || any;
                any = _unreliableStaticRpcCache.SendAcks(conn, stream) || any;

                if (!any)
                {
                    stream.ResetPosition();
                    continue;
                }

                var message = new BroascastAcks
                {
                    data = stream.ToByteData()
                };

                if (_asServer)
                    Send(conn, message, Channel.Unreliable);
                else SendToServer(message, Channel.Unreliable);
                stream.ResetPosition();
            }
        }

        private void OnAcks(Connection conn, BroascastAcks data, bool asServer)
        {
            using var stream = BitPackerPool.Get(data.data);

            _unreliableTypeCache.ReceiveAcks(conn, stream);
            _unreliableRpcCache.ReceiveAcks(conn, stream);
            _unreliableChildRpcCache.ReceiveAcks(conn, stream);
            _unreliableStaticRpcCache.ReceiveAcks(conn, stream);
        }

        public void OnConnected(Connection conn, bool asServer)
        {
        }

        public void OnDisconnected(Connection conn, bool asServer)
        {
            _reliableTypeCache.Clear(conn);
            _reliableChildRpcCache.Clear(conn);
            _reliableStaticRpcCache.Clear(conn);
            _reliableRpcCache.Clear(conn);

            _unreliableTypeCache.Clear(conn);
            _unreliableChildRpcCache.Clear(conn);
            _unreliableStaticRpcCache.Clear(conn);
            _unreliableRpcCache.Clear(conn);
        }
    }
}
