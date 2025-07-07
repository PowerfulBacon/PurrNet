using PurrNet;
using UnityEngine;

public class TrySimpleSpawner : NetworkBehaviour
{
    [SerializeField] private NetworkManager _networkManager;
    [SerializeField] private GameObject _playerPrefab;
    [SerializeField] private Transform _spawnPoint;

    protected override void OnEarlySpawn(bool asServer)
    {
        if (asServer)
            return;

        var playerObject = Instantiate(_playerPrefab, _spawnPoint.position, Quaternion.identity);
        var networkIdentity = playerObject.GetComponent<NetworkIdentity>();
        networkIdentity.GiveOwnership(localPlayer);
    }
}
