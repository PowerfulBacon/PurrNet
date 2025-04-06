using System;
using PurrNet.Packing;

namespace PurrNet.Profiler
{
    public readonly struct BroadcastSample : IDisposable
    {
        public readonly Type type;
        public readonly BitPacker data;

        public BroadcastSample(Type type, ArraySegment<byte> data)
        {
            this.type = type;
            this.data = BitPackerPool.Get();
            this.data.WriteBytes(data);
        }

        public void Dispose()
        {
            data?.Dispose();
        }
    }
}
