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
    [SerializeField] private bool _onSpawnException;
    [SerializeField] private bool _onSpawnVariantException;
    [SerializeField] private bool _onDespawnException;
    [SerializeField] private bool _onDespawnVariantException;

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
