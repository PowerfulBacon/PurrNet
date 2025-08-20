using PurrNet;
using PurrNet.Transports;
using UnityEngine;

[DefaultExecutionOrder(10000)]
public class Reconnect : MonoBehaviour
{
    [SerializeField] private NetworkManager _manager;

    private void OnEnable()
    {
        _manager.onClientConnectionState += OnClientConnectionState;
    }

    private void OnDisable()
    {
        _manager.onClientConnectionState -= OnClientConnectionState;
    }

    private void OnClientConnectionState(ConnectionState state)
    {
        if (state == ConnectionState.Disconnected)
            _manager.StartClient();
    }
}
