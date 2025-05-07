using JetBrains.Annotations;
using PurrNet;
using PurrNet.Packing;
using TriInspector;
using UnityEngine;

public class StaticRpcTest : NetworkIdentity
{
    public SyncVar<float> someFloat = new SyncVar<float>(ownerAuth: false);

    [Button("SendObserverRpc"), UsedImplicitly]
    public void SendRpc()
    {
        var someData = new SomeBaseData
        {
            someInt = 1,
            someString = "Hello"
        };

        var someData2 = new SomeDerivedData
        {
            someInt = 2,
            someString = "World",
            someInt2 = 3,
            someString2 = "!"
        };

        SomeBaseData someData3 = someData2;

        SendObserverRpcM(0, someData);
    }

    [ObserversRpc(bufferLast: true)]
    public void SendObserverRpcM<T>(int a, T someData) where T : SomeBaseData
    {
        Debug.Log($"SendObserverRpcM: {someData} {someData.GetType().Name} {typeof(T).Name}");
        throw new System.NotImplementedException();
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

public class SomeDerivedData : SomeBaseData, IPackedAuto
{
    public int someInt2;
    public string someString2;

    public override string ToString()
    {
        return $"{nameof(SomeDerivedData)}: {nameof(someInt)}: {someInt}, {nameof(someString)}: {someString}, {nameof(someInt2)}: {someInt2}, {nameof(someString2)}: {someString2}";
    }
}
