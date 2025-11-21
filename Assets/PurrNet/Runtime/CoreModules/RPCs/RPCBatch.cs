namespace PurrNet.Modules
{
    public class RPCBatch
    {
        public void Queue<T>(PlayerID target, T packet) where T : IRpc
        {
        }
    }
}
