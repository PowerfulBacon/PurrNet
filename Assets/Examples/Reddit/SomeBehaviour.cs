using System;
using PurrNet;
using UnityEngine;

public abstract class HealthShit : NetworkBehaviour
{
    public virtual void DealDamage(int damage)
    {
        Debug.Log($"Dealt {damage} damage from base class.");
    }
}

public class SomeBehaviour : HealthShit
{
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
            DealDamage(10);
    }

    [ObserversRpc(runLocally: true)]
    public override void DealDamage(int damage)
    {
        Debug.Log($"Dealt {damage} damage.");
        base.DealDamage(damage);
    }
}
