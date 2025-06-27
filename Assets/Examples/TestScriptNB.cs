using PurrNet;
using PurrNet.Logging;
using PurrNet.Packing;

namespace System.Runtime.CompilerServices
{
    internal static class IsExternalInit { }
}

public record PickedUpEvent(string ItemName, int Quantity) : IPackedAuto;

public class TestScriptNB : NetworkIdentity
{
    [PurrButton]
    public void ServerRpcTesTButton()
    {
        ServerRpcTesT(new PickedUpEvent("TestItem", 1));
    }

    protected override void OnSpawned()
    {
        if (isOwner)
            ServerRpcTesT(new PickedUpEvent("SpawnedItem", 1));
    }

    [ServerRpc]
    public void ServerRpcTesT(PickedUpEvent f, RPCInfo info = default)
    {
        PurrLogger.Log($"{f.ItemName} {f.Quantity} received from {info.sender} {isClient}");
        PurrLogger.Log($"ServerRpcTesT called from {info.sender} {isClient}");
        TargetRpcT(info.sender);
    }

    [TargetRpc]
    public void TargetRpcT(PlayerID target)
    {
        PurrLogger.Log($"TargetRpcT called on target {target}");
    }
}
