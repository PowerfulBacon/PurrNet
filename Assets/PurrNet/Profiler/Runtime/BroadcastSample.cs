using System;

namespace PurrNet.Profiler
{
    public struct BroadcastSample
    {
        public Type type;
        public int data;

        public BroadcastSample(Type type, int data)
        {
            this.type = type;
            this.data = data;
        }
    }
}
