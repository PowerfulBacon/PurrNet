using System;
using System.Collections.Generic;
using PurrNet.Logging;
using PurrNet.Packing;

namespace PurrNet.Modules
{
    public struct NetworkTransformBatch : IDisposable, IPackedAuto
    {
        public BitPacker packet;

        public void Dispose()
        {
            packet?.Dispose();
        }
    }
    
    public class NetworkTransformModule : INetworkModule, IFixedUpdate
    {
        private readonly List<NetworkTransform> _networkTransforms = new ();
        private readonly ScenePlayersModule _scenePlayers;
        private readonly PlayersBroadcaster _broadcaster;
        private readonly SceneID _scene;
        private bool _asServer;
        
        public NetworkTransformModule(PlayersBroadcaster broadcaster, ScenePlayersModule scenePlayers, SceneID scene)
        {
            _scenePlayers = scenePlayers;
            _broadcaster = broadcaster;
            _scene = scene;
        }

        public void Enable(bool asServer)
        {
            _asServer = asServer;
            _broadcaster.Subscribe<NetworkTransformBatch>(OnNetworkTransformBatch);
            
            if (asServer)
            {
                if (_scenePlayers.TryGetPlayersInScene(_scene, out var players))
                {
                    foreach (var player in players)
                        OnPlayerJoinedScene(player, _scene, true);
                }
                
                _scenePlayers.onPlayerLoadedScene += OnPlayerJoinedScene;
            }
            else
            {
                OnPlayerJoinedScene(PlayerID.Server, _scene, false);
            }
        }

        public void Disable(bool asServer)
        {
            _broadcaster.Unsubscribe<NetworkTransformBatch>(OnNetworkTransformBatch);
        }
        
        private void OnNetworkTransformBatch(PlayerID player, NetworkTransformBatch data, bool asServer)
        {
            using (data)
            {
                PurrLogger.Log("Received NetworkTransformBatch from " + player);
            }
        }
        
        private void OnPlayerJoinedScene(PlayerID player, SceneID scene, bool asserver)
        {
            if (_scene != scene)
                return;

            using var initialPacket = BitPackerPool.Get();
            PrepareFirstState(initialPacket, player);
            
            if (initialPacket.positionInBits == 0)
                return;

            var data = new NetworkTransformBatch { packet = initialPacket };
            
            if (asserver)
                 _broadcaster.Send(player, data);
            else _broadcaster.SendToServer(data);
        }

        private void PrepareFirstState(BitPacker packer, PlayerID player)
        {
            int ntCount = _networkTransforms.Count;

            if (player == PlayerID.Server)
            {
                for (var i = 0; i < ntCount; i++)
                {
                    var nt = _networkTransforms[i];
                    if (nt.isControlling)
                        nt.FullWrite(packer);
                }
            }
            else
            {
                for (var i = 0; i < ntCount; i++)
                {
                    var nt = _networkTransforms[i];
                    if (nt.isControlling && nt.observers.Contains(player))
                        nt.FullWrite(packer);
                }
            }
        }
        
        private void PrepareDeltaState(BitPacker packer, PlayerID player)
        {
            int ntCount = _networkTransforms.Count;

            if (player == PlayerID.Server)
            {
                for (var i = 0; i < ntCount; i++)
                {
                    var nt = _networkTransforms[i];
                    if (nt.isControlling)
                        nt.DeltaWrite(packer);
                }
            }
            else
            {
                for (var i = 0; i < ntCount; i++)
                {
                    var nt = _networkTransforms[i];
                    if (nt.isControlling && nt.observers.Contains(player))
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
            if (!_asServer)
            {
                using var packer = BitPackerPool.Get();
                PrepareDeltaState(packer, PlayerID.Server);
                
                if (packer.positionInBits != 0)
                    _broadcaster.SendToServer(new NetworkTransformBatch { packet = packer });
            }
            else if (_scenePlayers.TryGetPlayersInScene(_scene, out var players))
            {
                foreach (var player in players)
                {
                    using var packer = BitPackerPool.Get();
                    PrepareDeltaState(packer, player);
                    
                    if (packer.positionInBits != 0)
                        _broadcaster.Send(player, new NetworkTransformBatch { packet = packer });

                }
            }
        }
    }
}
