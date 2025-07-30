using System;
using PurrNet;
using UnityEngine;

[RegisterNetworkType(typeof(Texture))]
[RegisterNetworkType(typeof(Sprite))]
public class Test : NetworkIdentity
{
    [SerializeField] private GameObject spawnedObject;
    [SerializeField] private GameObject networkPrefab;
    [SerializeField] private GameObject singleplayerPrefab;

    private void Update()
    {
        TestReceive(5);
    }

    [ServerRpc]
    private void TestReceive(int data)
    {
        TestReceiveObservers(data);
    }

    [ObserversRpc]
    private void TestReceiveObservers(int data)
    {

    }

    [PurrButton]
    private void RunSpawned()
    {
        Spawned(spawnedObject.transform);
    }

    [PurrButton]
    private void RunNetwork()
    {
        Spawned(networkPrefab.transform);
    }

    [PurrButton]
    private void RunSingle()
    {
        Spawned(singleplayerPrefab.transform);
    }

    [ObserversRpc]
    private void Spawned(Transform obj)
    {
        Debug.Log($"Received from sender: {obj}");
    }
}
