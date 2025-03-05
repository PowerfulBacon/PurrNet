using PurrNet;
using UnityEngine;

public abstract class HealthShit : NetworkBehaviour
{
    public void TriggerDealDamage(int damage)
    {
        DealDamage(damage);
    }

    [ObserversRpc]
    public virtual void DealDamage(int damage)
    {
        Debug.Log($"HealthShit");
    }
}

public class SomeBehaviour : HealthShit
{
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
            TriggerDealDamage(10);
    }

    [ObserversRpc]
    public override void DealDamage(int damage)
    {
        PurrCompilerFlags.EnterLocalExecution();

        Debug.Log($"SomeBehaviour");
        base.DealDamage(damage);

        PurrCompilerFlags.ExitLocalExecution();
    }
}
