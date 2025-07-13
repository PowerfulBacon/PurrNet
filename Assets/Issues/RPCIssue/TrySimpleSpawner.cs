using PurrNet;
using UnityEngine;

public class TrySimpleSpawner : NetworkBehaviour
{
    [SerializeField] private NetworkManager _networkManager;
    [SerializeField] private GameObject _playerPrefab;
    [SerializeField] private Transform _spawnPoint;

    protected override void OnSpawned(bool asServer)
    {
        Debug.Log(
            "Test OnSpawned; "
            + $"I am: *{this.localPlayer}*; "
            + $"as server: *{asServer}*; "
            + $"is server: *{this.isServer}*; "
            + $"is client: *{this.isClient}*; "
            + $"is client+obs: *{this.isClientAndObserving}*"
        );

        this.SomeRpc();
    }

    [ObserversRpc(bufferLast: true)]
    private void SomeRpc(RPCInfo rpcInfo = default)
    {
        Debug.Log($"SomeRpc called by {rpcInfo.sender}");
    }

    protected override void OnEarlySpawn(bool asServer)
    {
        if (asServer)
            return;

        var playerObject = Instantiate(_playerPrefab, _spawnPoint.position, Quaternion.identity);
        var networkIdentity = playerObject.GetComponent<NetworkIdentity>();
        networkIdentity.GiveOwnership(localPlayer);
    }
}
