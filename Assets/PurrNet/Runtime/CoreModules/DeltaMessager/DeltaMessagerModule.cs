using System.Collections.Generic;
using PurrNet.Logging;
using PurrNet.Packing;
using PurrNet.Transports;
using PurrNet.Utils;

namespace PurrNet.Modules
{
    public class DeltaMessagerModule
    {
        readonly SceneID _scene;
        readonly PlayersBroadcaster _broadcaster;
        readonly ScenePlayersModule _scenePlayers;

        readonly Dictionary<PlayerID, DeltaIDs> _confirmedKeys = new();
        readonly DeltaCache _cache = new();

        public DeltaMessagerModule(SceneID scene, PlayersBroadcaster broadcaster, ScenePlayersModule scenePlayers)
        {
            _scene = scene;
            _broadcaster = broadcaster;
            _scenePlayers = scenePlayers;
        }

        public void Enable()
        {
            _scenePlayers.onPlayerUnloadedScene += OnPlayerUnloadedScene;
            _broadcaster.Subscribe<DeltaMessage>(OnDeltaMessage);
            _broadcaster.Subscribe<AckDeltaMessage>(OnAckDeltaMessage);
        }

        public void Disable()
        {
            _scenePlayers.onPlayerUnloadedScene -= OnPlayerUnloadedScene;
            _broadcaster.Unsubscribe<DeltaMessage>(OnDeltaMessage);
            _broadcaster.Unsubscribe<AckDeltaMessage>(OnAckDeltaMessage);
        }

        private void OnPlayerUnloadedScene(PlayerID player, SceneID scene, bool asserver)
            => _confirmedKeys.Remove(player);

        public void SendMessage<K, D>(PlayerID target, K keyData, D content) where K : struct where D : struct
        {
            var key = DeltaValue.FromValue(keyData);
            var data = DeltaValue.FromValue(content);

            var message = new DeltaMessage
            {
                scene = _scene,
                key = key,
                value = data,
                deltaWith = null,
                messageId = _cache.GetNextMessageId(key)
            };

            if (target.isServer)
                 _broadcaster.SendToServer(message, Channel.Unreliable);
            else _broadcaster.Send(target, message, Channel.Unreliable);
        }

        private void OnDeltaMessage(PlayerID player, DeltaMessage data, bool asServer)
        {
            if (data.scene != _scene) return;

            if (!data.deltaWith.HasValue)
            {
                _cache.Cache(data);
                return;
            }

            if (!_cache.TryGetDeltaMessage(data.key, data.messageId, out var deltaWithData))
            {
                PurrLogger.LogError(Hasher.TryGetType(data.key.type, out var keyType)
                    ? $"DeltaMessage with key of type `{keyType.Name}` not found in cache"
                    : $"DeltaMessage with id {data.messageId} not found in cache");
            }

            var oldData = deltaWithData.value;
            var delta = data.value;

            var newData = CalculateFromDelta(oldData, delta);
            var newMessage = new DeltaMessage
            {
                scene = _scene,
                key = data.key,
                messageId = data.messageId,
                deltaWith = data.deltaWith,
                value = newData
            };

            _cache.Cache(newMessage);
            data.Dispose();
        }

        static DeltaValue CalculateFromDelta(DeltaValue old, DeltaValue delta)
        {
            if (!Hasher.TryGetType(old.type, out var oldType))
            {
                PurrLogger.LogError(
                    $"Type with id `{old.type}` not registered; cannot calculate delta.");
                return default;
            }

            var result = BitPackerPool.Get();
            var oldData = old.Deserialize(oldType);

            var oldPos = delta.container.positionInBits;
            delta.container.ResetPositionAndMode(true);
            object newVal = null;
            DeltaPacker.Read(delta.container, oldType, oldData, ref newVal);
            delta.container.SetBitPosition(oldPos);

            Packer.Write(result, oldType, newVal);
            return DeltaValue.FromValue(oldType, newVal);
        }

        private void OnAckDeltaMessage(PlayerID player, AckDeltaMessage data, bool asServer)
        {
            if (data.scene != _scene) return;

            if (!_confirmedKeys.TryGetValue(player, out var confirmedKeys))
            {
                confirmedKeys = new DeltaIDs();
                _confirmedKeys[player] = confirmedKeys;
            }

            confirmedKeys.Comfirm(data.key, data.messageId);
        }
    }
}
