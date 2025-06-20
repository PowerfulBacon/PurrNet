using System.Collections.Generic;
using PurrNet;
using UnityEngine;

public class Test : NetworkIdentity
{
    [SerializeField] private Material _testMaterial;

    [PurrButton]
    private void SendMat()
    {
        TestRpc(_testMaterial);
    }
    
    [ObserversRpc]
    private void TestRpc(Material received)
    {
        Debug.Log($"Received material: {received.name}", received);
    }
}
