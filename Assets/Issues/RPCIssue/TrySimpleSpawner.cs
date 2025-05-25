using PurrNet;
using UnityEngine;

public class TrySimpleSpawner : MonoBehaviour
{
    [SerializeField] private NetworkManager _networkManager;
    [SerializeField] private GameObject _playerPrefab;
    [SerializeField] private Transform _spawnPoint;

    private void Awake()
    {
        _networkManager.onPlayerJoinedScene += OnPlayerJoinedScene;
    }

    private void OnPlayerJoinedScene(PlayerID player, SceneID scene, bool asServer)
    {
        if (!asServer)
            return;

        var playerObject = Instantiate(_playerPrefab, _spawnPoint.position, Quaternion.identity);
        var networkIdentity = playerObject.GetComponent<NetworkIdentity>();
        networkIdentity.GiveOwnership(player);
    }
}
