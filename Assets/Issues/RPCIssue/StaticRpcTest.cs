using PurrNet;
using PurrNet.Packing;
using TriInspector;
using UnityEngine;

public class StaticRpcTest : NetworkIdentity
{
    [ObserversRpc(runLocally: true), Button("SendObserverRpc")]
    public void SendObserverRpc()
    {
        Debug.Log("SendObserverRpc");
    }

    void Awake()
    {
        SomeData data = new SomeData();
        SomeDataStruct dataStruct = new SomeDataStruct();
    }
}

public struct SomeDataStruct : IPacked
{
    public int a;
    public int b;

    public void Read(BitPacker packer)
    {
        Packer<int>.Read(packer, ref a);
        Packer<int>.Read(packer, ref b);
    }

    public void Write(BitPacker packer)
    {
        Packer<int>.Write(packer, a);
        Packer<int>.Write(packer, b);
    }
}


public class SomeData : IPacked
{
    public int a;
    public int b;

    public void Read(BitPacker packer)
    {
        Packer<int>.Read(packer, ref a);
        Packer<int>.Read(packer, ref b);
    }

    public void Write(BitPacker packer)
    {
        Packer<int>.Write(packer, a);
        Packer<int>.Write(packer, b);
    }
}
