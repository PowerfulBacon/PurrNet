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
    }

    private void OnDestroy()
    {
        if (_networkManager != null)
            _networkManager.onClientSpawnValidate -= ValidateSpawn;
    }

    private bool ValidateSpawn(PlayerID player, SpawnPacket data)
    {
        if (data.TryGetRawPrefab(_networkManager, out var prefab))
        {
            PurrLogger.Log($"Spawn validated for player {player}: {data.prototype}\nPrefab: {prefab.name}");
            return true;
        }

        return false;
    }
}
