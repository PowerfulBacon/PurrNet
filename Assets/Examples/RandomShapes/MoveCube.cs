using PurrNet;
using PurrNet.Logging;
using UnityEngine;

public class MoveCube : NetworkBehaviour
{
    [SerializeField] private float _speed = 1f;
    [SerializeField] private float _xRange = 5f;
    [SerializeField] private float _yRange = 5f;
    
    float _noiseOffset;

    private void Awake()
    {
        _noiseOffset = Random.value * 100f;
    }

    protected override void OnSpawned()
    {
        this.enabled = isController;
    }

    protected override void OnDespawned()
    {
        PurrLogger.Log("Despawned");
    }

    protected override void OnDespawned(bool asServer)
    {
        PurrLogger.Log("Despawned as " + (asServer ? "server" : "client"));
    }

    void Update()
    {
        var xNoise = Mathf.PerlinNoise(Time.time * _speed + _noiseOffset, _noiseOffset) * _xRange - _xRange / 2f;
        var yNoise = Mathf.PerlinNoise(_noiseOffset, Time.time * _speed + _noiseOffset) * _yRange - _yRange / 2f;
        transform.position = new Vector3(xNoise, yNoise, 0f);
    }
}
