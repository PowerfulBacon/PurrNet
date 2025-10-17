using PurrNet;
using UnityEngine;

public class TestingDeltas : NetworkIdentity
{
    [SerializeField] private bool _onSpawnException;
    [SerializeField] private bool _onSpawnVariantException;
    [SerializeField] private bool _onDespawnException;
    [SerializeField] private bool _onDespawnVariantException;


    protected override void OnSpawned()
    {
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
