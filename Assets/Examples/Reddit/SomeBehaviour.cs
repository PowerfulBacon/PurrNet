using PurrNet;
using UnityEngine;

public class SomeBehaviour : NetworkBehaviour
{
    [ContextMenu("Send RPC and shit"), ServerRpc]
    void TestRPC()
    {
        Debug.Log("Received TestRPC!");
    }
}