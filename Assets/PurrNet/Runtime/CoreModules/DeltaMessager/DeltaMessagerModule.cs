using PurrNet.Transports;

namespace PurrNet.Modules
{
    public class DeltaMessagerModule
    {
        readonly SceneID _scene;
        readonly PlayersBroadcaster _broadcaster;
        readonly ScenePlayersModule _scenePlayers;
        readonly bool _asServer;

        public DeltaMessagerModule(SceneID scene, PlayersBroadcaster broadcaster, ScenePlayersModule scenePlayers, bool asServer)
        {
            _scene = scene;
            _broadcaster = broadcaster;
            _scenePlayers = scenePlayers;
            _asServer = asServer;
        }

        public void SendMessage(PlayerID target, ByteData key, ByteData data)
        {
            throw new System.NotImplementedException();
        }

        public void Enable()
        {
            throw new System.NotImplementedException();
        }

        public void Disable()
        {
            throw new System.NotImplementedException();
        }
    }
}
