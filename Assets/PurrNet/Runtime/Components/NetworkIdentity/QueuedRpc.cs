
using PurrNet;

public struct QueuedRpc
{
    public ChildRPCPacket packet;
    public RPCInfo rpcInfo;
    public bool asServer;

    public QueuedRpc(ChildRPCPacket packet, RPCInfo rpcInfo, bool asServer)
    {
        this.packet = packet;
        this.rpcInfo = rpcInfo;
        this.asServer = asServer;
    }
}
