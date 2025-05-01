using JetBrains.Annotations;
using PurrNet;
using TriInspector;
using UnityEngine;

public class StaticRpcTest : NetworkIdentity
{
    [Button("SendObserverRpc"), UsedImplicitly]
    public void SendRpc()
    {
        SendObserverRpc("Test with a bunch of data, this is a test, for testing and testing purposes and testing only");
    }

    [ObserversRpc(bufferLast: true)]
    public void SendObserverRpc(string someData)
    {
        Debug.Log("SendObserverRpc "+ someData);
    }
}
