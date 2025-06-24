using PurrNet;
using UnityEngine;

public class PrefabTest : NetworkIdentity
{
    [SerializeField] private SyncVar<int> _test = new (0, ownerAuth: true);

    public SyncVar<int> test => _test;

    [ServerRpc]
    public void Print(string message)
    {
        Debug.Log($"ServerRpc called with message: {message}");
    }
}
