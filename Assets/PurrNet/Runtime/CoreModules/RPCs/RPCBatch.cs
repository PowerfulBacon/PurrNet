namespace PurrNet.Modules
{
    public class RPCBatch
    {
        private readonly PlayersManager _playersManager;

        public RPCBatch(PlayersManager playersManager)
        {
            _playersManager = playersManager;
        }

        public void Queue<T>(PlayerID target, T packet) where T : IRpc
        {
            _playersManager.Send(target, packet);
        }
    }
}
