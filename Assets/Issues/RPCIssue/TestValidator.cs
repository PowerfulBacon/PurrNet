using System.Collections.Generic;
using PurrNet;
using PurrNet.Logging;
using PurrNet.Modules;
using UnityEngine;

public class TestValidator : MonoBehaviour
{
    [SerializeField] private NetworkManager _networkManager;

    private void Awake()
    {
        _networkManager.onClientSpawnValidate += ValidateSpawn;
        Test();
    }

    [ServerOnly]
    void Test()
    {
        PurrLogger.Log("Test", this);
    }

    private void OnDestroy()
    {
        if (_networkManager != null)
            _networkManager.onClientSpawnValidate -= ValidateSpawn;
    }

    private bool ValidateSpawn(PlayerID player, SpawnPacket data)
    {
        if (data.TryGetRawPrefab(_networkManager, out var prefab))
            return true;
        return false;
    }
}
