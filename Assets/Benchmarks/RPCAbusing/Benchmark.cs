using PurrNet;
using PurrNet.Transports;
using UnityEngine;

public abstract class Benchmark : NetworkIdentity, ITick
{
    [SerializeField] private float _duration = 10f;

    private float _time;
    private bool _running;
    private bool _done;

    private int _bytesSent;
    private int _bytesReceived;

    public virtual bool hasFinished => true;

    public virtual void OnBenchmarkStart() {}

    protected override void OnSpawned()
    {
        _time = 0;
        networkManager.transport.transport.onDataSent += OnDataSent;
        networkManager.transport.transport.onDataReceived += OnDataReceived;

        _bytesSent = 0;
        _bytesReceived = 0;
    }

    [ContextMenu("Start Benchmark"), PurrButton]
    public void StartBenchmark()
    {
        _running = true;
        OnBenchmarkStart();
    }

    protected override void OnDespawned()
    {
        networkManager.transport.transport.onDataSent -= OnDataSent;
        networkManager.transport.transport.onDataReceived -= OnDataReceived;
    }

    private void OnDataReceived(Connection conn, ByteData data, bool asServer)
    {
        if (!_running || _done)
            return;
        _bytesReceived += data.length;
    }

    private void OnDataSent(Connection conn, ByteData data, bool asServer)
    {
        if (!_running || _done)
            return;
        _bytesSent += data.length;
    }

    protected abstract void OnBenchmarkTick();

    public void OnTick(float delta)
    {
        if (!_running)
            return;

        if (_done)
            return;

        _time += delta;

        if (_time < _duration)
        {
            OnBenchmarkTick();
        }
        else if (hasFinished)
        {
            _done = true;

            float kbsSent = _bytesSent / 1024f;
            float kbsReceived = _bytesReceived / 1024f;
            float extraTime = _time - _duration;
            Debug.Log($"[{GetType().Name}] Sent: {kbsSent:0.00} Kb/s; Received: {kbsReceived:0.00} Kb/s; Extra time: {extraTime}");
        }
    }
}
