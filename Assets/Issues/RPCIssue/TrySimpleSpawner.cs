using System.Threading.Tasks;
using PurrNet;
using PurrNet.Logging;
using UnityEngine;

public class TrySimpleSpawner : NetworkBehaviour
{
    [SerializeField] private NetworkManager _networkManager;
    [SerializeField] private GameObject _playerPrefab;
    [SerializeField] private Transform _spawnPoint;

    protected override void OnSpawned(bool asServer)
    {
        if (!asServer)
            _ = HandleDefendPhase();
    }

    public Task HandleDefendPhase()
    {
        Debug.Log("HandleDefendPhase.");

        this?.EnterHang();
        return Task.CompletedTask;
    }

    [ObserversRpc(PurrNet.Transports.Channel.ReliableOrdered)]
    public void EnterHang()
    {
        PurrLogger.Log($"Player {owner} is entering hang state.", this);
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
