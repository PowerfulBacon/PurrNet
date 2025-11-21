namespace PurrNet.Modules
{
    public interface IRPCBatcher
    {
        void Queue<T>(PlayerID target, T packet) where T : IRpc;
    }
}
