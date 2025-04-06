using System;
using PurrNet.Packing;

namespace PurrNet.Profiler
{
    public readonly struct RpcsSample : IDisposable
    {
        public readonly Type type;
        public readonly string method;
        public readonly BitPacker data;
        public readonly UnityEngine.Object context;

        public RpcsSample(Type type, string method, ArraySegment<byte> data, UnityEngine.Object context)
        {
            this.type = type;
            this.method = method;
            this.context = context;
            this.data = BitPackerPool.Get();
            this.data.WriteBytes(data);
        }

        public void Dispose()
        {
            data?.Dispose();
        }
    }
}
