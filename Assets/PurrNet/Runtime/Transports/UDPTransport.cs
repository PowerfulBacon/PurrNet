using System.Collections.Generic;
using System.Net;
using PurrNet.Logging;
using PurrNet.Utils;
using Ruffles.Channeling;
using Ruffles.Configuration;
using Ruffles.Core;
using Ruffles.Utils;
using UnityEngine;

namespace PurrNet.Transports
{
    [DefaultExecutionOrder(-100)]
    public class UDPTransport : GenericTransport, ITransport
    {
        [Header("Server Settings")]
        [Tooltip("The port which the server will start on, and clients connect.")]
        [SerializeField]
        private ushort _serverPort = 5000;

        [Tooltip("The max amount of client connections allowed.")] [SerializeField]
        private int _maxConnections = 100;

        [Header("Client Settings")]
        [Tooltip("This is the IP the client will use to connect to the server.")]
        [SerializeField]
        private string _address = "127.0.0.1";

        [Header("Reliability Settings")]
        [SerializeField, Range(0.1f, 5f), PurrLock]
        [Tooltip("The factor to multiply the round trip time by to calculate the resend delay.\n" +
                 "If no ack is received in this time, the packet will be resent.")]
        private float _retryLatencyFactor = 1.8f;

        public event OnConnected onConnected;
        public event OnDisconnected onDisconnected;
        public event OnDataReceived onDataReceived;
        public event OnDataSent onDataSent;
        public event OnConnectionState onConnectionState;

        public string address
        {
            get => _address;
            set => _address = value;
        }

        public ushort serverPort
        {
            get => _serverPort;
            set => _serverPort = value;
        }

        public int maxConnections
        {
            get => _maxConnections;
            set => _maxConnections = value;
        }

        public float retryLatencyFactor
        {
            get => _retryLatencyFactor;
            set => _retryLatencyFactor = value;
        }

        public IReadOnlyList<Connection> connections => _connections;

        private RuffleSocket _client;
        private RuffleSocket _server;

        public ConnectionState clientState { get; private set; } = ConnectionState.Disconnected;

        public ConnectionState listenerState { get; private set; } = ConnectionState.Disconnected;

        readonly List<Connection> _connections = new List<Connection>();

        private readonly Dictionary<int, Ruffles.Connections.Connection> _connectionsMap = new ();
        private readonly Dictionary<Ruffles.Connections.Connection, int> _connectionsMapReverse = new ();

        public override bool isSupported => Application.platform != RuntimePlatform.WebGLPlayer;

        public override ITransport transport => this;

        private void Awake()
        {
            Ruffles.Utils.Logging.CurrentLogLevel = LogLevel.Error;
        }

        public void RaiseDataReceived(Connection conn, ByteData data, bool asServer)
        {
            onDataReceived?.Invoke(conn, data, asServer);
        }

        public void RaiseDataSent(Connection conn, ByteData data, bool asServer)
        {
            onDataSent?.Invoke(conn, data, asServer);
        }

        protected override void StartServerInternal()
        {
            Listen(_serverPort);
        }

        protected override void StartClientInternal()
        {
            Connect(_address, _serverPort);
        }

        private Ruffles.Connections.Connection _clientToServerConn;

        private int _nextConnectionId = 1;

        private void HandleEvent(NetworkEvent e, bool asServer)
        {
            switch (e.Type)
            {
                case NetworkEventType.Connect:
                {
                    if (asServer)
                    {
                        var connId = _nextConnectionId++;
                        var conn = new Connection(connId);
                        _connectionsMap[connId] = e.Connection;
                        _connectionsMapReverse[e.Connection] = connId;
                        _connections.Add(conn);

                        onConnected?.Invoke(conn, true);
                    }
                    else
                    {
                        _clientToServerConn = e.Connection;
                        clientState = ConnectionState.Connected;
                        TriggerConnectionStateEvent(false);
                        onConnected?.Invoke(new Connection(0), false);
                    }
                    break;
                }
                case NetworkEventType.Timeout:
                case NetworkEventType.Disconnect:
                {
                    var reason = e.Type == NetworkEventType.Timeout
                        ? DisconnectReason.Timeout
                        : DisconnectReason.ClientRequest;

                    if (_connectionsMapReverse.TryGetValue(e.Connection, out var connId))
                    {
                        var conn = new Connection(connId);
                        _connections.Remove(conn);
                        _connectionsMap.Remove(connId);
                        _connectionsMapReverse.Remove(e.Connection);
                        onDisconnected?.Invoke(conn, reason, asServer);
                    }

                    if (!asServer)
                    {
                        onDisconnected?.Invoke(new Connection(0), reason, false);

                        clientState = ConnectionState.Disconnecting;
                        TriggerConnectionStateEvent(false);
                        clientState = ConnectionState.Disconnected;
                        TriggerConnectionStateEvent(false);
                    }
                    break;
                }
                case NetworkEventType.Data:
                {
                    if (asServer)
                    {
                        if (!_connectionsMapReverse.TryGetValue(e.Connection, out var connId))
                            return;
                        var data = new ByteData(e.Data);
                        onDataReceived?.Invoke(new Connection(connId), data, true);
                    }
                    else
                    {
                        onDataReceived?.Invoke(new Connection(0), new ByteData(e.Data), false);
                    }
                    break;
                }
                case NetworkEventType.BroadcastData:
                case NetworkEventType.AckNotification:
                case NetworkEventType.UnconnectedData:
                case NetworkEventType.Nothing:
                    break;
                default:
                {
                    PurrLogger.LogError($"Unhandled event type: {e.Type}");
                    break;
                }
            }
        }

        /// In this mode you should use ManualReceive (without PollEvents) for receive packets
        /// and ManualUpdate(...) for update and send packets
        public void TickUpdate(float delta)
        {
            if (_server is { IsRunning: true })
            {
                while (true)
                {
                    var e = _server.Poll();

                    if (e.Type == NetworkEventType.Nothing)
                    {
                        e.Recycle();
                        break;
                    }

                    HandleEvent(e, true);
                    e.Recycle();
                }
            }

            if (_client is { IsRunning: true })
            {
                while (true)
                {
                    var e = _client.Poll();

                    if (e.Type == NetworkEventType.Nothing)
                    {
                        e.Recycle();
                        break;
                    }

                    HandleEvent(e, false);
                    e.Recycle();
                }
            }
        }

        public void UnityUpdate(float delta)
        {
            /*if (_server.IsRunning) _server.PollEvents();
            if (_client.IsRunning) _client.PollEvents();*/
        }

        public void Connect(string ip, ushort port)
        {
            if (clientState == ConnectionState.Connected)
                return;

            clientState = ConnectionState.Connecting;

            TriggerConnectionStateEvent(false);

            _client = new RuffleSocket(new SocketConfig
            {
                DualListenPort = 0,
                ChallengeDifficulty = 20, // Difficulty 20 is fairly hard
                ChannelTypes = channels,
                EnableBandwidthTracking = false,
                ReliabilityResendRoundtripMultiplier = _retryLatencyFactor,
                LogicDelay = 10,
                EnableSyncronizationEvent = false,
                EnableSyncronizedCallbacks = false
            });

            if (!_client.Start())
            {
                clientState = ConnectionState.Disconnecting;
                TriggerConnectionStateEvent(false);
                clientState = ConnectionState.Disconnected;
            }

            TriggerConnectionStateEvent(false);

            _client.Connect(new IPEndPoint(IPAddress.Parse(ip), port));
        }

        public void Disconnect()
        {
            if (clientState is not (ConnectionState.Connected or ConnectionState.Connecting))
                return;

            clientState = ConnectionState.Disconnecting;
            TriggerConnectionStateEvent(false);

            if (_clientToServerConn != null)
            {
                _clientToServerConn.Disconnect(true);
                _clientToServerConn = null;

                onDisconnected?.Invoke(new Connection(0), DisconnectReason.ClientRequest, false);
            }

            _client.Shutdown();
            _client = null;

            clientState = ConnectionState.Disconnected;
            TriggerConnectionStateEvent(false);
        }

        public void Listen(ushort port)
        {
            _server = new RuffleSocket(new SocketConfig
            {
                // LogicDelay =
                DualListenPort = port,
                ChallengeDifficulty = 20, // Difficulty 20 is fairly hard
                ChannelTypes = channels,
                EnableBandwidthTracking = false,
                ReliabilityResendRoundtripMultiplier = _retryLatencyFactor,
                LogicDelay = 10,
                EnableSyncronizationEvent = false,
                EnableSyncronizedCallbacks = false
            });

            if (listenerState is ConnectionState.Disconnected or ConnectionState.Disconnecting)
            {
                listenerState = ConnectionState.Connecting;
                TriggerConnectionStateEvent(true);

                bool started = _server.Start();

                if (started)
                {
                    listenerState = ConnectionState.Connected;
                }
                else
                {
                    listenerState = ConnectionState.Disconnecting;
                    TriggerConnectionStateEvent(true);
                    listenerState = ConnectionState.Disconnected;
                }

                TriggerConnectionStateEvent(true);
            }
        }

        public void StopListening()
        {
            if (listenerState is ConnectionState.Connected or ConnectionState.Connecting)
            {
                listenerState = ConnectionState.Disconnecting;
                TriggerConnectionStateEvent(true);

                foreach (var conn in _connectionsMap.Values)
                    conn.Disconnect(true);
                _server.Shutdown();

                listenerState = ConnectionState.Disconnected;
                TriggerConnectionStateEvent(true);

                _connections.Clear();
                _connectionsMap.Clear();
                _connectionsMapReverse.Clear();
            }

            _server = null;
        }

        static readonly ChannelType[] channels =
        {
            ChannelType.Reliable,
            ChannelType.ReliableSequenced,
            ChannelType.UnreliableOrdered,
            ChannelType.Unreliable,
            ChannelType.ReliableSequencedFragmented
        };

        static byte ToDeliveryMethod(Channel channel)
        {
            return channel switch
            {
                Channel.ReliableUnordered => 0,
                Channel.ReliableOrdered => 1,
                Channel.UnreliableSequenced => 2,
                Channel.Unreliable => 3,
                _ => 3
            };
        }

        private ulong _serverMessageId;

        public void SendToClient(Connection target, ByteData data, Channel method = Channel.Unreliable)
        {
            if (listenerState is not ConnectionState.Connected)
                return;

            if (!target.isValid)
                return;

            if (!_connectionsMap.TryGetValue(target.connectionId, out var connection))
                return;

            connection.Send(data.segment, ToDeliveryMethod(method), false, _serverMessageId++);
            RaiseDataSent(target, data, true);
        }

        private ulong _clientMessageId;

        public void SendToServer(ByteData data, Channel method = Channel.Unreliable)
        {
            if (clientState != ConnectionState.Connected)
                return;

            _clientToServerConn.Send(data.segment, ToDeliveryMethod(method), false, _clientMessageId++);
            RaiseDataSent(new Connection(0), data, false);
        }

        public void CloseConnection(Connection conn)
        {
            if (listenerState is ConnectionState.Connected)
            {
                if (_connectionsMap.TryGetValue(conn.connectionId, out var connection))
                    connection.Disconnect(true);
            }
        }

        private void OnDisable()
        {
            if (_clientToServerConn != null)
            {
                _clientToServerConn.Disconnect(true);
                _clientToServerConn = null;
            }

            Disconnect();
            StopListening();

            listenerState = ConnectionState.Disconnected;
            clientState = ConnectionState.Disconnected;

            TriggerConnectionStateEvent(true);
            TriggerConnectionStateEvent(false);

            _connections.Clear();
            _connectionsMap.Clear();
            _connectionsMapReverse.Clear();
        }

        ConnectionState _prevClientState = ConnectionState.Disconnected;
        ConnectionState _prevServerState = ConnectionState.Disconnected;

        private void TriggerConnectionStateEvent(bool asServer)
        {
            if (asServer)
            {
                if (_prevServerState != listenerState)
                {
                    onConnectionState?.Invoke(listenerState, true);
                    _prevServerState = listenerState;
                }
            }
            else
            {
                if (_prevClientState != clientState)
                {
                    onConnectionState?.Invoke(clientState, false);
                    _prevClientState = clientState;
                }
            }
        }
    }
}
