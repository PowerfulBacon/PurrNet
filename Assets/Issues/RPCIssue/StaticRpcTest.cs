using JetBrains.Annotations;
using PurrNet;
using TriInspector;
using UnityEngine;
using CompressionLevel = PurrNet.CompressionLevel;

public class StaticRpcTest : NetworkIdentity
{
    [Button("SendObserverRpc"), UsedImplicitly]
    public void SendRpc()
    {
        SendObserverRpc("Test with a bunch of data, this is a test, for testing and testing purposes and testing only");
    }

    [ObserversRpc(bufferLast: true, compressionLevel: CompressionLevel.Best)]
    public void SendObserverRpc(string someData)
    {
        throw new System.NotImplementedException();
        Debug.Log("SendObserverRpc "+ someData);
    }
}
