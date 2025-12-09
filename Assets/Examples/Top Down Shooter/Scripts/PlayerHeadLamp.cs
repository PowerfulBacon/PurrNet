using PurrNet;
using UnityEngine;

public class PlayerHeadLamp : NetworkIdentity
{
    public SyncVar<bool> _lampIsOn = new SyncVar<bool>(true, ownerAuth: true);

    protected override void OnSpawned()
    {
        if (isOwner)
        {
            Debug.Log("I am the owner, turning lamp off");
            _lampIsOn.value = false;
        }
    }

    [PurrButton]
    public void ToggleLamp()
    {
        _lampIsOn.value = !_lampIsOn.value;
    }
}
