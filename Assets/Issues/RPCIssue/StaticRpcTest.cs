using JetBrains.Annotations;
using PurrNet;
using TriInspector;
using UnityEngine;

public class StaticRpcTest : MonoBehaviour
{
    [ObserversRpc]
    public static void TestStaticRpc()
    {
        Debug.Log($"TestStaticRpc");
    }

    [Button("Trigger Static Rpc"), UsedImplicitly]
    public void TriggerTestStaticRpc()
    {
        TestStaticRpc();
    }
}
