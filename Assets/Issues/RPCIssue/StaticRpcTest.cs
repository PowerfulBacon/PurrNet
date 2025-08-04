using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using JetBrains.Annotations;
using PurrNet;
using PurrNet.Logging;
using PurrNet.Packing;
using UnityEngine;

public class CustomClass : IPackedAuto
{
    public string data1;
    public string data2;

    /*[DontPack]*/ public List<CustomClass> Neighbours;
    /*[DontPack]*/ public List<CustomClass> NeighboursNull;

    public override string ToString()
    {
        string result = $"data1: {data1}, data2: {data2}\n";
        if (Neighbours != null)
        {
            foreach (var n in Neighbours)
            {
                result += n + "\n";
            }
        }

        return result;
    }
}

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
    SyncList<CustomClass> _list = new ();

    private void Awake()
    {
        var test = new CustomClass
        {
            data1 = "Hello",
            data2 = "World",
            Neighbours = new List<CustomClass>
            {
                new CustomClass
                {
                    data1 = "Hello2",
                    data2 = "World2"
                },
                new CustomClass
                {
                    data1 = "Hello3",
                    data2 = "World3"
                }
            }
        };

        _list.Add(test);
    }

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
        CustomClass test = new CustomClass
        {
            data1 = "Hello",
            data2 = "World",
            Neighbours = new List<CustomClass>
            {
                new CustomClass
                {
                    data1 = "Hello2",
                    data2 = "World2"
                },
                new CustomClass
                {
                    data1 = "Hello3",
                    data2 = "World3"
                }
            }
        };

        ServerRpc(test);
    }

    [ServerRpc(requireOwnership: false)]
    public static void ServerRpc(CustomClass classy)
    {
        PurrLogger.Log(classy.ToString());
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
