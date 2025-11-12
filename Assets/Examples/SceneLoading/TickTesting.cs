using PurrNet;
using UnityEngine;

public class TickTesting : NetworkIdentity, ITick
{
    [SerializeField] private ulong _tick;

    public void OnTick(float delta)
    {
        ++_tick;
    }
}
