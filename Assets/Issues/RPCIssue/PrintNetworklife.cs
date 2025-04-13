using PurrNet;
using UnityEngine;

public class PrintNetworklife : NetworkIdentity
{
    protected override void OnSpawned()
    {
        Debug.Log("OnSpawned");
    }

    protected override void OnSpawned(bool asServer)
    {
        Debug.Log($"OnSpawned {asServer}");
    }

    protected override void OnDespawned()
    {
        Debug.Log("OnDespawned");
    }

    protected override void OnDespawned(bool asServer)
    {
        Debug.Log($"OnDespawned {asServer}");
    }
}
