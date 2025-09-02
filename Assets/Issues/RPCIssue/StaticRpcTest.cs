using System.Collections.Generic;
using System.Threading.Tasks;
using JetBrains.Annotations;
using PurrNet;
using PurrNet.Packing;
using PurrNet.Pooling;
using UnityEngine;

public class StaticRpcTest : NetworkIdentity
{
    [SerializeField] List<ulong> _players;

    [PurrButton("SendTargetRpc"), UsedImplicitly]
    public void SendTargetRpc()
    {
        if (owner.HasValue)
            TargetRpc(owner.Value);
    }

    [PurrButton("Send to list"), UsedImplicitly]
    public void SendServerRpcList()
    {
        using var players = DisposableList<PlayerID>.Create();
        foreach (var player in _players)
            players.Add(new PlayerID(player, false));
        SendObserverRpcM(players, 123456789);
    }

    [PurrButton("Send to all observer"), UsedImplicitly]
    public void SendServerRpcNoOwner()
    {
        SendObserverRpcM(observers, 123456789);
    }

    [TargetRpc(bufferLast: true)]
    public void SendObserverRpcM<T>(IReadOnlyList<PlayerID> players, T data)
    {
        Debug.Log($"SendObserverRpcM: {data}");
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

public class SomeBaseData : IPackedAuto, IStandaloneSerializable
{
    public int someInt;
    public string someString;

    public override string ToString()
    {
        return $"{nameof(SomeBaseData)}: {nameof(someInt)}: {someInt}, {nameof(someString)}: {someString}";
    }
}
