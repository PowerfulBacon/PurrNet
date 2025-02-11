using System.Collections.Generic;
using PurrNet.Logging;
using PurrNet.Packing;
using PurrNet.Transports;

namespace PurrNet.Modules
{
    public struct NetworkTransformDelta : IPackedAuto
    {
        public readonly ByteData packet;
        
        public NetworkTransformDelta(BitPacker packer)
        {
            packet = packer.ToByteData();
        }
    }
    
    public class NetworkTransformModule : INetworkModule, IFixedUpdate
    {
        private readonly List<NetworkTransform> _networkTransforms = new ();
        private readonly ScenePlayersModule _scenePlayers;
        private readonly PlayersBroadcaster _broadcaster;
        private readonly NetworkManager _manager;
        private readonly SceneID _scene;
        private bool _asServer;
        
        public NetworkTransformModule(NetworkManager manager, PlayersBroadcaster broadcaster, ScenePlayersModule scenePlayers, SceneID scene)
        {
            _manager = manager;
            _scenePlayers = scenePlayers;
            _broadcaster = broadcaster;
            _scene = scene;
        }

        public void Enable(bool asServer)
        {
            _asServer = asServer;
            _broadcaster.Subscribe<NetworkTransformDelta>(OnNetworkTransformDelta);
        }

        public void Disable(bool asServer)
        {
            _broadcaster.Unsubscribe<NetworkTransformDelta>(OnNetworkTransformDelta);
        }

        private void OnNetworkTransformDelta(PlayerID player, NetworkTransformDelta data, bool asServer)
        {
            using var packet = BitPackerPool.Get(data.packet);
            int ntCount = _networkTransforms.Count;

            if (asServer)
            {
                for (var i = 0; i < ntCount; i++)
                {
                    var nt = _networkTransforms[i];
                    if (nt.IsControlling(player, false))
                        nt.DeltaRead(packet);
                }
            }
            else
            {
                for (var i = 0; i < ntCount; i++)
                {
                    var nt = _networkTransforms[i];
                    if (!nt.IsControlling(nt.localPlayerForced, false))
                        nt.DeltaRead(packet);
                }
            }
        }
        
        public PlayerID GetLocalPlayer()
        {
            if (_manager.TryGetModule<PlayersManager>(false, out var _players))
                return _players.localPlayerId.GetValueOrDefault();
            return PlayerID.Server;
        }

        private void PrepareDeltaState(BitPacker packer, PlayerID player)
        {
            var localPlayer = GetLocalPlayer();
            int ntCount = _networkTransforms.Count;

            if (player == PlayerID.Server)
            {
                for (var i = 0; i < ntCount; i++)
                {
                    var nt = _networkTransforms[i];
                    if (nt.IsControlling(localPlayer, false))
                        nt.DeltaWrite(packer);
                }
            }
            else
            {
                for (var i = 0; i < ntCount; i++)
                {
                    var nt = _networkTransforms[i];
                    if (!nt.IsControlling(player, false) && nt.observers.Contains(player))
                        nt.DeltaWrite(packer);
                }
            }
        }
        
        public void Register(NetworkTransform networkTransform)
        {
            // TODO: Make sure this order is always the same for all clients
            _networkTransforms.Add(networkTransform);
        }
        
        public void Unregister(NetworkTransform networkTransform)
        {
            _networkTransforms.Remove(networkTransform);
        }

        public void FixedUpdate()
        {
            var localPlayer = GetLocalPlayer();
            int ntCount = _networkTransforms.Count;
            
            for (var i = 0; i < ntCount; i++)
            {
                var nt = _networkTransforms[i];
                if (nt.IsControlling(localPlayer, _asServer))
                    nt.GatherState();
            }

            if (!_asServer)
            {
                using var packer = BitPackerPool.Get();
                PrepareDeltaState(packer, PlayerID.Server);

                if (packer.positionInBits != 0)
                    _broadcaster.SendToServer(new NetworkTransformDelta(packer));
            }
            else if (_scenePlayers.TryGetPlayersInScene(_scene, out var players))
            {
                foreach (var player in players)
                {
                    if (player == localPlayer)
                        continue;
                    
                    using var packer = BitPackerPool.Get();
                    PrepareDeltaState(packer, player);

                    if (packer.positionInBits != 0)
                        _broadcaster.Send(player, new NetworkTransformDelta(packer));
                }
            }
            
            for (var i = 0; i < ntCount; i++)
            {
                var nt = _networkTransforms[i];
                if (nt.IsControlling(localPlayer, _asServer))
                    nt.DeltaSave();
            }
        }
    }
}
