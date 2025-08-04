using System.Threading.Tasks;
using JetBrains.Annotations;
using PurrNet;
using PurrNet.Packing;
using UnityEngine;

public static class StaticRpcTest2<T>
{
    [ObserversRpc(bufferLast: true)]
    public static void StaticMethod(T data)
    {
        Debug.Log($"StaticMethod called with data: {data}");
    }
}

public class StaticRpcTest : NetworkIdentity
{
    [PurrButton("SendObserverRpc"), UsedImplicitly]
    public void SendRpc()
    {
        var someData = new SomeBaseDataB
        {
            someInt = 1,
            someString = "Hello",
            someInt2 = 5,
            someString2 = "World"
        };

        SendObserverRpcM(72, someData, 42);
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
    public static void ServerRpc()
    {
        Debug.Log($"ServerRpc");
    }

    [ObserversRpc(bufferLast: true)]
    public static void SendObserverRpcM<T>(int a, T someData, int b) where T : SomeBaseData
    {
        Debug.Log($"SendObserverRpcM: {a}");
        Debug.Log($"SendObserverRpcM: {someData} {someData?.GetType().Name}");
        Debug.Log($"SendObserverRpcM: {b}");
    }

    [TargetRpc(requireServer: false)]
    public Task TargetRpc(PlayerID player, RPCInfo info = default)
    {
        Debug.Log($"TargetRpc from {info.sender}");
        return Task.CompletedTask;
    }
}

public class SomeBaseDataB : SomeBaseData
{
    public int someInt2;
    public string someString2;

    public void FUCKU(ref SomeBaseDataB fef)
    {
        SomeBaseData a = new SomeBaseData();
        fef = (SomeBaseDataB)a;
    }

    public override string ToString()
    {
        return $"{nameof(SomeBaseDataB)}: {nameof(someInt2)}: {someInt2}, {nameof(someString2)}: {someString2}";
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
