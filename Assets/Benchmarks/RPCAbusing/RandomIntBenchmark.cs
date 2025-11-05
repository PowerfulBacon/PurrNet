using System;
using System.Threading.Tasks;
using PurrNet;
using PurrNet.Modules;
using PurrNet.Packing;
using PurrNet.Transports;
using UnityEngine;
using Random = UnityEngine.Random;

public enum BenchmarkType
{
    Normal,
    NormalDelta,
    Generic,
    GenericDelta,
    Returnable,
    ReturnableDelta,
    GenericReturnable,
    GenericReturnableDelta,
}

public class RandomIntBenchmark : Benchmark
{
    [SerializeField] private BenchmarkType _type;
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
            var v = Random.Range(_minRandomValue, _maxRandomValue);
            ++_sent;

            switch (_type)
            {
                case BenchmarkType.Normal:
                    Normal(v);
                    break;
                case BenchmarkType.NormalDelta:
                    NormalDelta(v);
                    break;
                case BenchmarkType.Generic:
                    Generic(v);
                    break;
                case BenchmarkType.GenericDelta:
                    GenericDelta(v);
                    break;
                case BenchmarkType.Returnable:
                    for (var o = 0; o < observers.Count; o++)
                        _ = Returnable(observers[o], v);
                    break;
                case BenchmarkType.ReturnableDelta:
                    for (var o = 0; o < observers.Count; o++)
                        _ = ReturnableDelta(observers[o], v);
                    break;
                case BenchmarkType.GenericReturnable:
                    for (var o = 0; o < observers.Count; o++)
                        _ = ReturnableGeneric(observers[o], v);
                    break;
                case BenchmarkType.GenericReturnableDelta:
                    for (var o = 0; o < observers.Count; o++)
                        _ = ReturnableGenericDelta(observers[o], v);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }

    public override void OnBenchmarkStart()
    {
        _received = 0;
        _sent = 0;
    }

    [TargetRpc(deltaPacked: false)]
    Task<int> Returnable(PlayerID target, int value)
    {
        ++_received;
        return Task.FromResult(value);
    }

    [TargetRpc(deltaPacked: true)]
    Task<int> ReturnableDelta(PlayerID target, int value)
    {
        ++_received;
        return Task.FromResult(value);
    }

    [TargetRpc(deltaPacked: false)]
    Task<T> ReturnableGeneric<T>(PlayerID target, T value)
    {
        ++_received;
        Debug.Log(value);
        return Task.FromResult(value);
    }

    [TargetRpc(deltaPacked: true)]
    Task<T> ReturnableGenericDelta<T>(PlayerID target, T value)
    {
        ++_received;
        Debug.Log(value);
        return Task.FromResult(value);
    }

    [ObserversRpc(deltaPacked: false)]
    void Normal(int someRandomValue)
    {
        ++_received;
    }

    [ObserversRpc(deltaPacked: true)]
    void NormalDelta(int someRandomValue)
    {
        ++_received;
    }

    [ObserversRpc(deltaPacked: false)]
    void Generic<T>(T someRandomValue)
    {
        ++_received;
    }

    [ObserversRpc(deltaPacked: true)]
    void GenericDelta<T>(T someRandomValue)
    {
        ++_received;
    }
}
