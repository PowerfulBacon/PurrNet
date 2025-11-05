using System;
using System.Reflection;

namespace PurrNet.Modules
{
    internal readonly struct RPCKey : IEquatable<RPCKey>
    {
        private readonly IReflect type;
        private readonly byte rpcId;

        public override int GetHashCode()
        {
            return type.GetHashCode() ^ rpcId.GetHashCode();
        }

        public RPCKey(IReflect type, byte rpcId)
        {
            this.type = type;
            this.rpcId = rpcId;
        }

        public bool Equals(RPCKey other)
        {
            return Equals(type, other.type) && rpcId == other.rpcId;
        }

        public override bool Equals(object obj)
        {
            return obj is RPCKey other && Equals(other);
        }
    }
}