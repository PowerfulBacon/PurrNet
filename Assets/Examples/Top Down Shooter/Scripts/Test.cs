using System;
using PurrNet;
using UnityEngine;

public class Test : NetworkIdentity
{
    [SerializeField] private int _localHealth = 100;

    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.X))
            SetHealth(_localHealth - 10);
    }

    [ObserversRpc(bufferLast: true)]
    private void SetHealth(int newHealth)
    {
        _localHealth = newHealth;
    }
}
