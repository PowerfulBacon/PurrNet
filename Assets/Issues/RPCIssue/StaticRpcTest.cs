using JetBrains.Annotations;
using PurrNet;
using PurrNet.Packing;
using TriInspector;
using UnityEngine;

public class StaticRpcTest : NetworkIdentity
{
    [Button("SendObserverRpc"), UsedImplicitly]
    public void SendRpc()
    {
        var someData = new SomeBaseData
        {
            someInt = 1,
            someString = "Hello"
        };

        SendObserverRpcM(0, someData);
    }

    [ObserversRpc(bufferLast: true)]
    public void SendObserverRpcM<T>(int a, T someData) where T : SomeBaseData
    {
        Debug.Log($"SendObserverRpcM: {someData} {someData.GetType().Name} {typeof(T).Name}");
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
