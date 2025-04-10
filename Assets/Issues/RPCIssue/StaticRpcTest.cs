using PurrNet;
using TriInspector;
using UnityEngine;

public class StaticRpcTest : NetworkIdentity
{
    [ObserversRpc(runLocally: true), Button("SendObserverRpc")]
    public void SendObserverRpc()
    {
        Debug.Log("SendObserverRpc");
    }
}
