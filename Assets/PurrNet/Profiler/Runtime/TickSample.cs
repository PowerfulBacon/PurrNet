using System.Collections.Generic;

namespace PurrNet.Profiler
{
    public class TickSample
    {
        public readonly List<RpcsSample> receivedRpcs = new ();
        public readonly List<RpcsSample> sentRpcs = new ();
        public readonly List<BroadcastSample> receivedBroadcasts = new ();
        public readonly List<BroadcastSample> sentBroadcasts = new ();
        public readonly List<int> forwardedBytes = new ();
    }
}
