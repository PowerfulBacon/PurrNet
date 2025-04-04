using System;

namespace PurrNet.Profiler
{
    public struct RpcsSample
    {
        public Type type;
        public string method;
        public int data;
        public UnityEngine.Object context;

        public RpcsSample(Type type, string method, int data, UnityEngine.Object context)
        {
            this.type = type;
            this.method = method;
            this.data = data;
            this.context = context;
        }
    }
}
