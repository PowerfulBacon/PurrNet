using System;
using System.Collections.Generic;

namespace PurrNet.Profiler
{
    public static class Statistics
    {
        public const int MAX_SAMPLES = 256;

        public static readonly List<TickSample> samples = new ();
        private static TickSample _currentSample = new ();

        public static event Action onSampleEnded;

        public static bool paused;

        public static void ReceivedBroadcast(Type type, int bytesReceived)
        {
            if (paused) return;
            _currentSample.receivedBroadcasts.Add(new BroadcastSample(type, bytesReceived));
        }

        public static void SentBroadcast(Type type, int bytesSent)
        {
            if (paused) return;
            _currentSample.sentBroadcasts.Add(new BroadcastSample(type, bytesSent));
        }

        public static void ForwardedBytes(int bytesSent)
        {
            if (paused) return;
            _currentSample.forwardedBytes.Add(bytesSent);
        }

        public static void ReceivedRPC(Type type, string method, int bytesReceived, UnityEngine.Object context)
        {
            if (paused) return;
            _currentSample.receivedRpcs.Add(new RpcsSample(type, method, bytesReceived, context));
        }

        public static void SentRPC(Type type, string method, int bytesSent, UnityEngine.Object context)
        {
            if (paused) return;
            _currentSample.sentRpcs.Add(new RpcsSample(type, method, bytesSent, context));
        }

        public static void MarkEndOfSampling()
        {
            if (paused) return;

            if (samples.Count >= MAX_SAMPLES)
                samples.RemoveAt(0);

            samples.Add(_currentSample);
            _currentSample = new TickSample();

            onSampleEnded?.Invoke();
        }
    }
}
