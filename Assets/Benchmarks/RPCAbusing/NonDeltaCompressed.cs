using PurrNet;
using UnityEngine;


public class NonDeltaCompressed : Benchmark
{
    [SerializeField] private int _rpcCount = 1000;
    [SerializeField] private int _minRandomValue;
    [SerializeField] private int _maxRandomValue = 1000;

    private int _received;
    private int _sent;

    public override bool hasFinished
    {
        get
        {
            if (isClient)
                return _received == _sent;
            return true;
        }
    }

    protected override void OnBenchmarkTick()
    {
        for (int i = 0; i < _rpcCount; i++)
        {
            ++_sent;
            SendRandomInteger(Random.Range(_minRandomValue, _maxRandomValue));
        }
    }

    public override void OnBenchmarkStart()
    {
        _received = 0;
        _sent = 0;
    }

    [ObserversRpc(deltaPacked: false)]
    void SendRandomInteger(int someRandomValue)
    {
        ++_received;
    }
}
