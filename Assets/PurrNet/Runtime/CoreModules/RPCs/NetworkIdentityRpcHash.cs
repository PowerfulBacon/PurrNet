using PurrNet.Utils;

namespace PurrNet.Modules
{
    internal readonly struct NetworkIdentityRpcHash<T> : IStableHashable
    {
        private readonly NetworkID id;
        private readonly SceneID scene;
        private readonly int rpcId;
        private readonly ulong offset;

        public NetworkIdentityRpcHash(RPCPacket context, ulong offset)
        {
            id = context.networkId;
            scene = context.sceneId;
            this.rpcId = context.rpcId;
            this.offset = offset;
        }

        public uint GetStableHash()
        {
            ulong nid = id.id.value;
            ulong nscope = id.scope.id.value;
            ulong sceneScope = scene.id.value;
            ulong rpc = (ulong)rpcId;

            ulong hash = 1469598103934665603UL;
            const ulong prime = 1099511628211UL;

            hash ^= Hasher<T>.stableHash;
            hash *= prime;

            hash ^= nid;
            hash *= prime;

            hash ^= nscope;
            hash *= prime;

            hash ^= sceneScope;
            hash *= prime;

            hash ^= rpc;
            hash *= prime;

            hash ^= offset;
            hash *= prime;

            return (uint)(hash ^ (hash >> 32));
        }
    }
}
