using PurrNet;
using UnityEngine;

public class PrintNetworklife : NetworkIdentity
{
    protected override void OnSpawned()
    {
        Debug.Log($"OnSpawned {this}", this);
    }

    protected override void OnSpawned(bool asServer)
    {
        Debug.Log($"OnSpawned {this} {asServer}", this);
    }

    protected override void OnDespawned()
    {
        Debug.Log($"OnDespawned {this}", this);
    }

    protected override void OnDespawned(bool asServer)
    {
        Debug.Log($"OnDespawned {this} {asServer}", this);
    }
}
