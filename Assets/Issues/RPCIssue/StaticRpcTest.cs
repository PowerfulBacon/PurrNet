using System.Threading.Tasks;
using JetBrains.Annotations;
using PurrNet;
using PurrNet.Packing;
using UnityEngine;

public class StaticRpcTest : NetworkIdentity
{
    [PurrButton("SendObserverRpc"), UsedImplicitly]
    public void SendRpc()
    {
        var someData = new SomeBaseData
        {
            someInt = 1,
            someString = "Hello"
        };

        SendObserverRpcM(0, someData);
    }

    [PurrButton("SendTargetRpc"), UsedImplicitly]
    public void SendTargetRpc()
    {
        if (owner.HasValue)
            TargetRpc(owner.Value);
    }

    [PurrButton("SendServerRpc"), UsedImplicitly]
    public void SendServerRpcNoOwner()
    {
        ServerRpc();
    }

    [ServerRpc(requireOwnership: false)]
    public void ServerRpc()
    {
        Debug.Log($"ServerRpc");
    }

    [ObserversRpc(bufferLast: true)]
    public void SendObserverRpcM<T>(int a, T someData) where T : SomeBaseData
    {
        Debug.Log($"SendObserverRpcM: {someData} {someData.GetType().Name} {typeof(T).Name}");
    }

    [TargetRpc(requireServer: false)]
    public Task TargetRpc(PlayerID player, RPCInfo info = default)
    {
        Debug.Log($"TargetRpc from {info.sender}");
        return Task.CompletedTask;
    }
}

public class SomeBaseData : IPackedAuto
{
    public int someInt;
    public string someString;

    public override string ToString()
    {
        return $"{nameof(SomeBaseData)}: {nameof(someInt)}: {someInt}, {nameof(someString)}: {someString}";
    }
}
