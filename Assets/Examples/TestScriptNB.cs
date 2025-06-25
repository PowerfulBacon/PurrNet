using PurrNet;
using PurrNet.Logging;

public class TestScriptNB : NetworkIdentity
{
    [PurrButton]
    public void ServerRpcTesTButton()
    {
        ServerRpcTesT();
    }

    protected override void OnSpawned(bool asServer)
    {
        if (!asServer)
            ServerRpcTesT();
    }

    [ServerRpc]
    public void ServerRpcTesT(RPCInfo info = default)
    {
        PurrLogger.Log($"ServerRpcTesT called from {info.sender} {isClient} {localPlayer}");
        TargetRpcT(info.sender);
    }

    [TargetRpc]
    public void TargetRpcT(PlayerID target)
    {
        PurrLogger.Log($"TargetRpcT called on target {target}");
    }
}
