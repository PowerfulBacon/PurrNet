
using PurrNet;
using UnityEngine;

public class TestModuleBacon : NetworkBehaviour
{

    [ObserversRpc]
    public void Test()
    {
        Debug.Log("Test");
    }

}
