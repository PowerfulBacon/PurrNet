using PurrNet;
using UnityEngine;

public class PrintNetworklife : NetworkIdentity
{
    [SerializeField] private bool _spawnPrint = true;
    [SerializeField] private bool _ownerPrint = true;

    protected override void OnSpawned()
    {
        if (_spawnPrint)
            Debug.Log($"OnSpawned {this}", this);
    }

    protected override void OnSpawned(bool asServer)
    {
        if (_spawnPrint)
            Debug.Log($"OnSpawned {this} {asServer}", this);
    }

    protected override void OnDespawned()
    {
        if (_spawnPrint)
            Debug.Log($"OnDespawned {this}", this);
    }

    protected override void OnDespawned(bool asServer)
    {
        if (_spawnPrint)
            Debug.Log($"OnDespawned {this} {asServer}", this);
    }

    protected override void OnOwnerChanged(PlayerID? oldOwner, PlayerID? newOwner, bool asServer)
    {
        if (_ownerPrint)
            Debug.Log($"OnOwnerChanged {this} {oldOwner} -> {newOwner} {asServer}", this);
    }
}
