using PurrNet;
using PurrNet.Packing;
using PurrNet.Pooling;
using UnityEngine;

public interface ISomeInterface : IPackedAuto
{
    public void DoSomething();
}

public struct SomeImpl : ISomeInterface
{
    public void DoSomething()
    {
        Debug.Log("SomeImpl");
    }
}

public class TestingDeltas : NetworkIdentity
{
    [SerializeField] private SyncBigData _bigData;
    [SerializeField] private bool _onSpawnException;
    [SerializeField] private bool _onSpawnVariantException;
    [SerializeField] private bool _onDespawnException;
    [SerializeField] private bool _onDespawnVariantException;

    [PurrButton]
    public void SetRandomData()
    {
        byte[] data = new byte[1024 * 1024];
        for (var i = 0; i < data.Length; i++)
            data[i] = (byte)Random.Range(0, 256);
        _bigData.SetData(data);
    }

    [PurrButton]
    public void PrintStartAndEndOfData()
    {
        var data = _bigData.data;

        string result = "";

        for (var i = 0; i < data.Length; i++)
        {
            result += data[i].ToString("X2") + "|";
            if (i > 5 && i < data.Length - 5)
            {
                result += "...";
                i = data.Length - 5;
            }
        }

        Debug.Log(result);
    }

    [ObserversRpc(bufferLast: true)]
    private void SendSomething(DisposableList<ISomeInterface> list, ISomeInterface data)
    {
        using (list)
        {
            for (var i = 0; i < list.Count; i++)
                list[i].DoSomething();
        }
    }

    protected override void OnSpawned()
    {
        using var data = DisposableList<ISomeInterface>.Create();
        data.Add(new SomeImpl());
        SendSomething(data, null);

        if (_onSpawnException)
            throw new System.NotImplementedException();
    }

    protected override void OnSpawned(bool asServer)
    {
        if (_onSpawnVariantException)
            throw new System.NotImplementedException();
    }

    protected override void OnDespawned()
    {
        if (_onDespawnException)
            throw new System.NotImplementedException();
    }

    protected override void OnDespawned(bool asServer)
    {
        if (_onDespawnVariantException)
            throw new System.NotImplementedException();
    }
}
